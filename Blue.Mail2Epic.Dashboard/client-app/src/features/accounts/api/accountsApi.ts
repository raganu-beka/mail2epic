import client from '../../../api/client';
import type {UserAccount} from "../types/models.ts";

export const getAll = async (): Promise<UserAccount[]> => {
    const response = await client.get('/accounts');
    return response.data;
};

export const create = async (email: string, isActive: boolean, isAdmin: boolean): Promise<UserAccount> => {
    const response = await client.post('/accounts', {email, isActive, isAdmin});
    return response.data;
}

export const update = async (id: number, isActive: boolean, isAdmin: boolean): Promise<UserAccount> => {
    const response = await client.patch(`/accounts/${id}`, {isActive, isAdmin});
    return response.data;
}

export const remove = async (id: number): Promise<void> => {
    const response = await client.delete(`/accounts/${id}`);
    return response.data;
}

export const getMe = async (): Promise<UserAccount> => {
    const response = await client.get('/accounts/me');
    return response.data;
}
