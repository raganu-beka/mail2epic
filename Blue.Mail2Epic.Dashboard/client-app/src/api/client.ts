import axios from 'axios';

const axiosInstance = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

axiosInstance.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

axiosInstance.interceptors.response.use(
    (response) => response,
    (error) => {
        const isAuthRequest = error.config?.url?.includes('auth');
        
        if (error.response?.status === 401 && !isAuthRequest) {
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export function extractApiError(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
    if (err && typeof err === 'object' && 'response' in err) {
        const data = (err as { response?: { data?: unknown } }).response?.data;
        if (data && typeof data === 'object' && 'message' in data && typeof (data as { message: unknown }).message === 'string') {
            return (data as { message: string }).message;
        }
        if (typeof data === 'string' && data.length > 0) return data;
    }
    return fallback;
}

export default axiosInstance;