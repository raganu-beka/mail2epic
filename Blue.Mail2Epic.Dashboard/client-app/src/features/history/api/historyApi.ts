import client from '../../../api/client';
import type {HistoryRow} from '../types/models';

export const getHistory = async (page: number, maxResults: number): Promise<HistoryRow[]> => {
    const response = await client.get('/history', { params: { page, maxResults } });
    return response.data;
};

