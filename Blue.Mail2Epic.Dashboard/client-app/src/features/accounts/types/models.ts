export interface UserAccount {
    id: number;
    email: string;
    displayName: string | null;
    isActive: boolean;
    isAdmin: boolean;
    lastUpdatedAt: string;
    lastLoginAt: string | null;
    googleMailboxAccountConnected: boolean;
    lastEmailProcessingAt: string | null;
}