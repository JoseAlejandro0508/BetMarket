import axios from 'axios';
import type {
  AuthResponse,
  DepositRequest,
  OddsOffer,
  OddsSyncSettings,
  UserBet,
  UserProfile,
  WalletSummary,
  WithdrawalRequest,
  WithdrawalSettings,
} from './types';

const API_BASE_URL = 'http://localhost:5144/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
});

export const setAuthToken = (token: string | null) => {
  if (token) {
    api.defaults.headers.common.Authorization = `Bearer ${token}`;
  } else {
    delete api.defaults.headers.common.Authorization;
  }
};

export const authApi = {
  register: async (payload: { displayName: string; email: string; password: string }) => {
    const { data } = await api.post<AuthResponse>('/auth/register', payload);
    return data;
  },
  login: async (payload: { email: string; password: string }) => {
    const { data } = await api.post<AuthResponse>('/auth/login', payload);
    return data;
  },
  me: async () => {
    const { data } = await api.get<UserProfile>('/auth/me');
    return data;
  },
};

export const oddsApi = {
  getOffers: async (sportKey?: string) => {
    const { data } = await api.get<OddsOffer[]>('/odds/offers', { params: sportKey ? { sportKey } : undefined });
    return data;
  },
  getSyncSettings: async () => {
    const { data } = await api.get<OddsSyncSettings>('/odds/sync-settings');
    return data;
  },
  updateSyncSettings: async (payload: {
    autoRefreshEnabled: boolean;
    refreshIntervalSeconds: number;
    sportKey: string;
  }) => {
    const { data } = await api.put<OddsSyncSettings>('/odds/sync-settings', payload);
    return data;
  },
  refreshNow: async (sportKey?: string) => {
    const { data } = await api.post<OddsSyncSettings>('/odds/refresh', null, {
      params: sportKey ? { sportKey } : undefined,
    });
    return data;
  },
};

export const betsApi = {
  placeBet: async (payload: {
    eventId: string;
    sportKey: string;
    homeTeam: string;
    awayTeam: string;
    commenceTime: string;
    bookmakerKey: string;
    bookmakerTitle: string;
    marketKey: string;
    selectedOutcomeName: string;
    selectedOutcomePrice: number;
    stake: number;
  }) => {
    const { data } = await api.post<UserBet>('/bets', payload);
    return data;
  },
  myBets: async () => {
    const { data } = await api.get<UserBet[]>('/bets/mine');
    return data;
  },
  pendingBets: async () => {
    const { data } = await api.get<UserBet[]>('/bets/pending');
    return data;
  },
  resolveBet: async (betId: string, isWon: boolean) => {
    const { data } = await api.patch<UserBet>(`/bets/${betId}/resolve`, { isWon });
    return data;
  },
};

export const walletApi = {
  summary: async () => {
    const { data } = await api.get<WalletSummary>('/wallet/summary');
    return data;
  },
  deposits: async () => {
    const { data } = await api.get<DepositRequest[]>('/wallet/deposits');
    return data;
  },
  createDeposit: async (payload: { amount: number; transactionId: string }) => {
    const { data } = await api.post<DepositRequest>('/wallet/deposits', payload);
    return data;
  },
  withdrawals: async () => {
    const { data } = await api.get<WithdrawalRequest[]>('/wallet/withdrawals');
    return data;
  },
  createWithdrawal: async (payload: { amount: number }) => {
    const { data } = await api.post<WithdrawalRequest>('/wallet/withdrawals', payload);
    return data;
  },
  getWithdrawalSettings: async () => {
    const { data } = await api.get<WithdrawalSettings>('/wallet/withdrawal-settings');
    return data;
  },
  updateWithdrawalSettings: async (payload: { method: string; account: string; displayName?: string }) => {
    const { data } = await api.put<WithdrawalSettings>('/wallet/withdrawal-settings', payload);
    return data;
  },
};
