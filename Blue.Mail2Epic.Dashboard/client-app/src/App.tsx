import './App.css'

import {BrowserRouter, Navigate, Route, Routes, useNavigate} from 'react-router-dom'
import {LoginPage} from './features/accounts/pages/LoginPage'
import {AccountsPage} from './features/accounts/pages/AccountsPage'
import {HistoryPage} from './features/history/pages/HistoryPage'
import {useState} from 'react'
import {UserInfoCard} from './features/accounts/components/UserInfoCard'

function parseJwtPayload(token: string): Record<string, unknown> {
    try {
        const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
        return JSON.parse(atob(base64));
    } catch {
        return {};
    }
}

function isAdmin(): boolean {
    const token = localStorage.getItem('token');
    if (!token) return false;
    const payload = parseJwtPayload(token);
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    return role === 'admin';
}

type Tab = 'history' | 'accounts';

function MainPage() {
    const admin = isAdmin();
    const [tab, setTab] = useState<Tab>('history');
    const navigate = useNavigate();

    if (!localStorage.getItem('token')) {
        return <Navigate to="/login" replace />;
    }

    function logout() {
        localStorage.removeItem('token');
        navigate('/login');
    }

    return (
        <div className="min-h-screen flex flex-col bg-gray-50">
            <nav className="bg-blue-600 text-white shadow-md">
                <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between gap-4">
                    <div className="flex items-center gap-2">
                        <span className="text-lg font-semibold tracking-tight select-none">Mail2Epic</span>
                        <span className="text-blue-200 text-sm hidden sm:inline">Dashboard</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => setTab('history')}
                            className={`px-3 py-1.5 rounded text-sm font-medium transition-colors ${tab === 'history' ? 'bg-white text-blue-700' : 'text-white hover:bg-blue-700'}`}
                        >
                            History
                        </button>
                        {admin && (
                            <button
                                onClick={() => setTab('accounts')}
                                className={`px-3 py-1.5 rounded text-sm font-medium transition-colors ${tab === 'accounts' ? 'bg-white text-blue-700' : 'text-white hover:bg-blue-700'}`}
                            >
                                Accounts
                            </button>
                        )}
                    </div>
                    <div className="flex items-center gap-3">
                        <UserInfoCard />
                        <button
                            onClick={logout}
                            className="px-3 py-1.5 rounded text-sm font-medium bg-blue-700 hover:bg-blue-800 transition-colors"
                        >
                            Logout
                        </button>
                    </div>
                </div>
            </nav>
            <main className="flex-1 max-w-7xl mx-auto w-full px-4 py-6">
                {tab === 'history' && <HistoryPage />}
                {tab === 'accounts' && admin && <AccountsPage />}
            </main>
        </div>
    );
}

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/" element={<MainPage />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App
