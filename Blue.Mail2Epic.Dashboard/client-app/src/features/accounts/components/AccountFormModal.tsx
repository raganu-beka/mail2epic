import {useState} from 'react';
import type {UserAccount} from '../types/models.ts';
import {extractApiError} from '../../../api/client.ts';

interface AccountFormModalProps {
    account: UserAccount | null;
    onClose: () => void;
    onSave: (email: string, isActive: boolean, isAdmin: boolean) => Promise<void>;
}

export function AccountFormModal({ account, onClose, onSave }: AccountFormModalProps) {
    const [email, setEmail] = useState(account?.email ?? '');
    const [isActive, setIsActive] = useState(account?.isActive ?? true);
    const [isAdmin, setIsAdmin] = useState(account?.isAdmin ?? false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const isEditMode = account !== null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            await onSave(email, isActive, isAdmin);
            onClose();
        } catch (err) {
            setError(extractApiError(err));
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-5">
                    {isEditMode ? 'Edit Account' : 'Add Account'}
                </h2>
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                        {isEditMode
                            ? <p className="text-sm text-gray-800">{email}</p>
                            : <input
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                maxLength={50}
                                required
                                disabled={loading}
                                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-60"
                                placeholder="user@example.com"
                            />
                        }
                    </div>
                    <div className="flex items-center gap-2">
                        <input
                            id="active"
                            type="checkbox"
                            checked={isActive}
                            onChange={(e) => setIsActive(e.target.checked)}
                            disabled={loading}
                            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                        <label htmlFor="active" className="text-sm text-gray-700">Active</label>
                    </div>
                    <div className="flex items-center gap-2">
                        <input
                            id="admin"
                            type="checkbox"
                            checked={isAdmin}
                            onChange={(e) => setIsAdmin(e.target.checked)}
                            disabled={loading}
                            className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                        />
                        <label htmlFor="admin" className="text-sm text-gray-700">Admin</label>
                    </div>
                    {error && (
                        <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
                    )}
                    <div className="flex justify-end gap-2 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={loading}
                            className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={loading}
                            className="px-4 py-2 text-sm font-medium rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-60 transition-colors"
                        >
                            {loading ? 'Saving…' : 'Save'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

