import client from '../../../api/client';

export const startGoogleLogin = (): void => {
    window.location.href = '/api/auth/google/start';
};

export const exchangeGoogleLogin =
    async (exchange: string): Promise<{ token: string }> => {
        const response = await client.post('/auth/google/exchange-google-login', { exchange });
        return response.data;
    };