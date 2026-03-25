import {useEffect, useState} from 'react';
import {useNavigate, useSearchParams} from 'react-router-dom';
import {exchangeGoogleLogin, startGoogleLogin} from '../api/authApi';

export function LoginPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const exchange = searchParams.get('exchange');
        if (!exchange) return;

        setLoading(true);
        exchangeGoogleLogin(exchange)
            .then(({token}) => {
                localStorage.setItem('token', token);
                navigate('/');
            })
            .catch(() => {
                setError('Login failed. Please try again.');
                setLoading(false);
            });
    }, [searchParams, navigate]);

    const handleGoogleLogin = () => {
        startGoogleLogin();
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
            <div className="bg-white rounded-2xl shadow-md p-8 w-full max-w-sm text-center">
                <div className="mb-6">
                    <h1 className="text-2xl font-bold text-gray-900">Mail2Epic</h1>
                    <p className="text-gray-500 text-sm mt-1">Sign in to your dashboard</p>
                </div>
                {error && (
                    <p className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
                )}
                <button
                    onClick={handleGoogleLogin}
                    disabled={loading}
                    className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white font-medium py-2.5 px-4 rounded-lg transition-colors"
                >
                    {loading ? 'Logging in…' : 'Continue with Google'}
                </button>
            </div>
        </div>
    );
}
