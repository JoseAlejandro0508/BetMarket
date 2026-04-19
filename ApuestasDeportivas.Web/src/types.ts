export type Role = 'User' | 'Admin';

export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  balance: number;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: UserProfile;
  roles: Role[];
}

export interface OutcomeOdds {
  name: string;
  price: number;
}

export interface MarketOdds {
  key: string;
  lastUpdate: string;
  outcomes: OutcomeOdds[];
}

export interface BookmakerOdds {
  key: string;
  title: string;
  lastUpdate: string;
  markets: MarketOdds[];
}

export interface OddsOffer {
  eventId: string;
  sportKey: string;
  commenceTime: string;
  homeTeam: string;
  awayTeam: string;
  bookmaker: BookmakerOdds;
  syncedAt?: string;
}

export interface OddsSyncSettings {
  autoRefreshEnabled: boolean;
  currentSportKey: string;
  refreshIntervalSeconds: number;
  lastRefreshAt: string | null;
  lastError: string;
  selectedSportKeys: string[];
}

export interface AvailableSport {
  key: string;
  group: string;
  title: string;
  description: string;
}

export interface UserBet {
  id: string;
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
  status: 'Pending' | 'Won' | 'Lost';
  createdAt: string;
  resolvedAt: string | null;
  userId: string;
  userDisplayName: string;
  userEmail: string;
}

export interface DepositRequest {
  id: string;
  amount: number;
  transactionId: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  createdAt: string;
}

export interface WithdrawalRequest {
  id: string;
  amount: number;
  method: 'Cup' | 'Mlc' | 'QvaPay';
  account: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  createdAt: string;
}

export interface WithdrawalSettings {
  method: 'Cup' | 'Mlc' | 'QvaPay';
  account: string;
  updatedAt: string;
}

export interface WalletSummary {
  totalDeposited: number;
  pendingDeposits: number;
  totalWithdrawn: number;
  pendingWithdrawals: number;
}
