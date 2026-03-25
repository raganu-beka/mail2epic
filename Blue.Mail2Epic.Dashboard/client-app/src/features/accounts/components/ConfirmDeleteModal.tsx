interface ConfirmDeleteModalProps {
    email: string;
    error: string;
    loading: boolean;
    onConfirm: () => void;
    onCancel: () => void;
}

export function ConfirmDeleteModal({ email, error, loading, onConfirm, onCancel }: ConfirmDeleteModalProps) {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-2">Delete Account</h2>
                <p className="text-sm text-gray-600 mb-5">
                    Are you sure you want to delete <span className="font-medium text-gray-800">{email}</span>? This cannot be undone.
                </p>
                {error && (
                    <p className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
                )}
                <div className="flex justify-end gap-2">
                    <button
                        type="button"
                        onClick={onCancel}
                        disabled={loading}
                        className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        onClick={onConfirm}
                        disabled={loading}
                        className="px-4 py-2 text-sm font-medium rounded-lg bg-red-600 text-white hover:bg-red-700 disabled:opacity-60 transition-colors"
                    >
                        {loading ? 'Deleting…' : 'Delete'}
                    </button>
                </div>
            </div>
        </div>
    );
}

