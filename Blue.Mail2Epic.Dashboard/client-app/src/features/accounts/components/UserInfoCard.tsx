import {useEffect, useRef, useState} from 'react';
import type {UserAccount} from '../types/models.ts';
import * as accountsApi from '../api/accountsApi.ts';

const CACHE_TTL_MS = 60_000;

interface CacheEntry { data: UserAccount; expiresAt: number; }
let cache: CacheEntry | null = null;

function getCached(): UserAccount | null {
    if (!cache) return null;
    if (Date.now() > cache.expiresAt) { cache = null; return null; }
    return cache.data;
}

function setCached(data: UserAccount) {
    cache = { data, expiresAt: Date.now() + CACHE_TTL_MS };
}

export function UserInfoCard() {
    const [user, setUser] = useState<UserAccount | null>(getCached());
    const [error, setError] = useState('');
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    const fetchMe = async () => {
        const cached = getCached();
        if (cached) {
            setUser(cached);
            return;
        }
        try {
            const data = await accountsApi.getMe();
            setCached(data);
            setUser(data);
            setError('');
        } catch {
            setError('Failed to load user info.');
        }
    };

    useEffect(() => {
        fetchMe();

        const schedule = () => {
            timerRef.current = setTimeout(async () => {
                cache = null;
                await fetchMe();
                schedule();
            }, CACHE_TTL_MS);
        };
        schedule();

        return () => {
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, []);

    const formatDate = (value: string | null) => {
        if (!value) return '—';
        const d = new Date(value);
        const dd = String(d.getDate()).padStart(2, '0');
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const yyyy = d.getFullYear();
        const HH = String(d.getHours()).padStart(2, '0');
        const MM = String(d.getMinutes()).padStart(2, '0');
        return `${dd}-${mm}-${yyyy} ${HH}:${MM}`;
    };

    if (error) return <span className="text-blue-200 text-xs">{error}</span>;
    if (!user) return <span className="text-blue-200 text-xs">Loading…</span>;

    return (
        <div className="text-right hidden sm:block">
            <p className="text-sm font-medium text-white leading-none">{user.displayName ?? user.email}</p>
            <p className="text-xs text-blue-200 mt-0.5">
                Last read: {formatDate(user.lastEmailProcessingAt)}
            </p>
        </div>
    );
}

