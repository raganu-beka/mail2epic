import {useEffect, useState} from 'react';
import type {UserAccount} from '../types/models.ts';
import * as accountsApi from '../api/accountsApi.ts';
import {AccountTable} from '../components/AccountTable.tsx';
import {AccountFormModal} from '../components/AccountFormModal.tsx';
import {ConfirmDeleteModal} from '../components/ConfirmDeleteModal.tsx';
import {extractApiError} from '../../../api/client.ts';

const RELOAD_COOLDOWN_MS = 10_000;

export function AccountsPage() {
    const [accounts, setAccounts] = useState<UserAccount[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedAccount, setSelectedAccount] = useState<UserAccount | null>(null);
    const [reloadCooldown, setReloadCooldown] = useState(false);

    const [accountToDelete, setAccountToDelete] = useState<UserAccount | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteError, setDeleteError] = useState('');

    const fetchAccounts = async () => {
        setLoading(true);
        setError('');
        try {
            const data = await accountsApi.getAll();
            setAccounts(data);
        } catch {
            setError('Failed to load accounts.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchAccounts();
    }, []);

    const handleReload = () => {
        if (reloadCooldown) return;
        fetchAccounts();
        setReloadCooldown(true);
        setTimeout(() => setReloadCooldown(false), RELOAD_COOLDOWN_MS);
    };

    const handleAdd = () => {
        setSelectedAccount(null);
        setIsModalOpen(true);
    };

    const handleEdit = (account: UserAccount) => {
        setSelectedAccount(account);
        setIsModalOpen(true);
    };

    const handleDeleteRequest = (account: UserAccount) => {
        setDeleteError('');
        setAccountToDelete(account);
    };

    const handleDeleteConfirm = async () => {
        if (!accountToDelete) return;
        setDeleteLoading(true);
        setDeleteError('');
        try {
            await accountsApi.remove(accountToDelete.id);
            setAccountToDelete(null);
            await fetchAccounts();
        } catch (err) {
            setDeleteError(extractApiError(err));
        } finally {
            setDeleteLoading(false);
        }
    };

    const handleDeleteCancel = () => {
        setAccountToDelete(null);
        setDeleteError('');
    };

    const handleSave = async (email: string, isActive: boolean, isAdmin: boolean) => {
        if (selectedAccount) {
            await accountsApi.update(selectedAccount.id, isActive, isAdmin);
        } else {
            await accountsApi.create(email, isActive, isAdmin);
        }
        await fetchAccounts();
    };

    const handleClose = () => {
        setIsModalOpen(false);
        setSelectedAccount(null);
    };

    return (
        <div>
            <div className="flex items-center justify-between mb-4">
                <h1 className="text-xl font-semibold text-gray-900">User Accounts</h1>
                <div className="flex gap-2">
                    <button
                        onClick={handleReload}
                        disabled={reloadCooldown}
                        className="px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                    >
                        Reload
                    </button>
                    <button
                        onClick={handleAdd}
                        className="px-3 py-1.5 text-sm font-medium rounded-lg bg-blue-600 text-white hover:bg-blue-700 transition-colors"
                    >
                        + Add Account
                    </button>
                </div>
            </div>
            {loading
                ? <p className="text-gray-500 text-sm">Loading…</p>
                : error
                    ? <p className="text-red-600 text-sm bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
                    : <AccountTable
                        accounts={accounts}
                        onEdit={handleEdit}
                        onDelete={handleDeleteRequest}
                    />
            }
            {isModalOpen && (
                <AccountFormModal
                    account={selectedAccount}
                    onClose={handleClose}
                    onSave={handleSave}
                />
            )}
            {accountToDelete && (
                <ConfirmDeleteModal
                    email={accountToDelete.email}
                    error={deleteError}
                    loading={deleteLoading}
                    onConfirm={handleDeleteConfirm}
                    onCancel={handleDeleteCancel}
                />
            )}
        </div>
    );
}
