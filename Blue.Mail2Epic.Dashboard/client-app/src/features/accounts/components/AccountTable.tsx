import type {UserAccount} from '../types/models.ts';

interface AccountTableProps {
    accounts: UserAccount[];
    onEdit: (account: UserAccount) => void;
    onDelete: (account: UserAccount) => void;
}

export function AccountTable({ accounts, onEdit, onDelete }: AccountTableProps) {

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

    return (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                    <thead className="bg-gray-50 border-b border-gray-200">
                        <tr>
                            {['Email', 'Name', 'Active', 'Admin', 'Gmail Connected', 'Last Login', 'Last Email Processed', 'Last Updated', 'Actions'].map(h => (
                                <th key={h} className="px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide whitespace-nowrap">{h}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                        {accounts.map((account) => (
                            <tr key={account.id} className="hover:bg-gray-50 transition-colors">
                                <td className="px-4 py-3 text-gray-800">{account.email}</td>
                                <td className="px-4 py-3 text-gray-600">{account.displayName ?? <span className="text-gray-400">—</span>}</td>
                                <td className="px-4 py-3">{account.isActive ? '✅' : '❌'}</td>
                                <td className="px-4 py-3">{account.isAdmin ? '✅' : '❌'}</td>
                                <td className="px-4 py-3">{account.googleMailboxAccountConnected ? '✅' : '❌'}</td>
                                <td className="px-4 py-3 text-gray-500 text-xs whitespace-nowrap">{formatDate(account.lastLoginAt)}</td>
                                <td className="px-4 py-3 text-gray-500 text-xs whitespace-nowrap">{formatDate(account.lastEmailProcessingAt)}</td>
                                <td className="px-4 py-3 text-gray-500 text-xs whitespace-nowrap">{formatDate(account.lastUpdatedAt)}</td>
                                <td className="px-4 py-3 whitespace-nowrap">
                                    <div className="flex gap-2">
                                        <button
                                            onClick={() => onEdit(account)}
                                            className="px-2.5 py-1 text-xs font-medium rounded border border-blue-300 text-blue-700 hover:bg-blue-50 transition-colors"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            onClick={() => onDelete(account)}
                                            className="px-2.5 py-1 text-xs font-medium rounded border border-red-300 text-red-600 hover:bg-red-50 transition-colors"
                                        >
                                            Delete
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
