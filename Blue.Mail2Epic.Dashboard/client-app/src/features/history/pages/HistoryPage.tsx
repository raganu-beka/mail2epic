import {useEffect, useRef, useState} from 'react';
import type {HistoryRow} from '../types/models';
import * as historyApi from '../api/historyApi';
import {HistoryTable} from '../components/HistoryTable';

const PAGE_SIZE = 25;

const CACHE_TTL_MS = 60_000;
const RELOAD_COOLDOWN_MS = 10_000;

interface CacheEntry { data: HistoryRow[]; expiresAt: number; }
const cache = new Map<number, CacheEntry>();

function getCached(page: number): HistoryRow[] | null {
    const entry = cache.get(page);
    if (!entry) return null;
    if (Date.now() > entry.expiresAt) { cache.delete(page); return null; }
    return entry.data;
}

function setCached(page: number, data: HistoryRow[]) {
    cache.set(page, { data, expiresAt: Date.now() + CACHE_TTL_MS });
}

export function HistoryPage() {
    const [rows, setRows] = useState<HistoryRow[]>([]);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [reloadCooldown, setReloadCooldown] = useState(false);
    const abortRef = useRef<AbortController | null>(null);

    const fetchHistory = async (p: number) => {
        const cached = getCached(p);
        if (cached) {
            setRows(cached);
            setLoading(false);
            setError('');
            return;
        }

        abortRef.current?.abort();
        abortRef.current = new AbortController();

        setLoading(true);
        setError('');
        try {
            const data = await historyApi.getHistory(p, PAGE_SIZE);
            setCached(p, data);
            setRows(data);
        } catch {
            setError('Failed to load history.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchHistory(page);
    }, [page]);

    const handlePrev = () => setPage(p => Math.max(1, p - 1));
    const handleNext = () => setPage(p => p + 1);

    const handleReload = () => {
        if (reloadCooldown) return;
        cache.delete(page);
        fetchHistory(page);
        setReloadCooldown(true);
        setTimeout(() => setReloadCooldown(false), RELOAD_COOLDOWN_MS);
    };

    return (
        <div>
            <div className="flex items-center justify-between mb-4">
                <h1 className="text-xl font-semibold text-gray-900">Email History</h1>
                <button
                    onClick={handleReload}
                    disabled={reloadCooldown}
                    className="px-3 py-1.5 text-sm font-medium rounded-lg border border-blue-600 text-blue-600 hover:bg-blue-50 disabled:opacity-50 transition-colors"
                >
                    Reload
                </button>
            </div>
            {loading
                ? <p className="text-gray-500 text-sm">Loading…</p>
                : error
                    ? <p className="text-red-600 text-sm bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
                    : <HistoryTable
                        rows={rows}
                        page={page}
                        pageSize={PAGE_SIZE}
                        onPrev={handlePrev}
                        onNext={handleNext}
                    />
            }
        </div>
    );
}
