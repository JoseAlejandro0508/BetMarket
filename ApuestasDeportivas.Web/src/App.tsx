import { useEffect, useMemo, useState } from 'react';
import { authApi, betsApi, oddsApi, setAuthToken, walletApi } from './api';
import type {
  AuthResponse,
  DepositRequest,
  OddsOffer,
  OddsSyncSettings,
  OutcomeOdds,
  UserBet,
  UserProfile,
  WalletSummary,
  WithdrawalRequest,
  WithdrawalSettings,
} from './types';

type AuthTab = 'login' | 'register';
type ThemeMode = 'light' | 'dark';
type Section = 'bets' | 'deposits' | 'withdrawals' | 'settings' | 'admin';
type PaymentMethod = 'CUP' | 'MLC' | 'QvaPay';
type IconName = 'sun' | 'moon' | 'settings' | 'logout' | 'bets' | 'deposits' | 'withdrawals' | 'admin';

interface SelectedBet {
  offer: OddsOffer;
  outcome: OutcomeOdds;
}

const THEME_STORAGE_KEY = 'theme-mode';

const formatSportLabel = (sportKey: string) => {
  if (!sportKey) return 'Deporte';
  return sportKey
    .replace(/^soccer_/, 'Fútbol · ')
    .replace(/^basketball_/, 'Baloncesto · ')
    .replace(/^americanfootball_/, 'Fútbol Americano · ')
    .replace(/^baseball_/, 'Béisbol · ')
    .replace(/^icehockey_/, 'Hockey · ')
    .replace(/^tennis_/, 'Tenis · ')
    .replace(/^mma_/, 'MMA · ')
    .replace(/^boxing_/, 'Boxeo · ')
    .replace(/^upcoming$/, 'Próximos')
    .replaceAll('_', ' ');
};

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 2,
  }).format(value);

const formatDateTime = (value: string) =>
  new Intl.DateTimeFormat('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));

const betStatusLabel: Record<UserBet['status'], string> = {
  Pending: 'Pendiente',
  Won: 'Ganada',
  Lost: 'Perdida',
};

const requestStatusLabel = (status: 'Pending' | 'Approved' | 'Rejected') => {
  if (status === 'Pending') return 'Pendiente';
  if (status === 'Approved') return 'Aprobado';
  return 'Rechazado';
};

const Icon = ({ name }: { name: IconName }) => {
  const paths: Record<IconName, string> = {
    sun: 'M12 4V2m0 20v-2m8-8h2M2 12h2m13.66 5.66 1.41 1.41M4.93 4.93l1.41 1.41m11.32-1.41-1.41 1.41M6.34 17.66l-1.41 1.41M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8Z',
    moon: 'M21 12.8A9 9 0 1 1 11.2 3a7 7 0 1 0 9.8 9.8Z',
    settings: 'M12 8.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Zm7.4 3.5-.9-.5.1-1-1.7-3-.9.3-.8-.7-.2-1h-3.5l-.2 1-.8.7-.9-.3-1.7 3 .1 1-.9.5v2l.9.5-.1 1 1.7 3 .9-.3.8.7.2 1h3.5l.2-1 .8-.7.9.3 1.7-3-.1-1 .9-.5v-2Z',
    logout: 'M15 17l5-5-5-5M20 12H9M12 19H6a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2h6',
    bets: 'M4 7h16M6 3h12a2 2 0 0 1 2 2v14l-4-2-4 2-4-2-4 2V5a2 2 0 0 1 2-2Z',
    deposits: 'M12 3v18M3 12h18',
    withdrawals: 'M12 3v18M6 9l6-6 6 6',
    admin: 'M12 2 3 7v6c0 5.2 3.8 8.9 9 10 5.2-1.1 9-4.8 9-10V7l-9-5Z',
  };

  return (
    <svg className="icon" viewBox="0 0 24 24" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d={paths[name]} />
    </svg>
  );
};

const toApiMethod = (method: PaymentMethod): WithdrawalSettings['method'] => {
  if (method === 'CUP') return 'Cup';
  if (method === 'MLC') return 'Mlc';
  return 'QvaPay';
};

const fromApiMethod = (method: WithdrawalSettings['method']): PaymentMethod => {
  if (method === 'Cup') return 'CUP';
  if (method === 'Mlc') return 'MLC';
  return 'QvaPay';
};

export function App() {
  const [authTab, setAuthTab] = useState<AuthTab>('login');
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [roles, setRoles] = useState<string[]>(() => JSON.parse(localStorage.getItem('roles') ?? '[]'));
  const [user, setUser] = useState<UserProfile | null>(() => {
    const raw = localStorage.getItem('user');
    return raw ? (JSON.parse(raw) as UserProfile) : null;
  });
  const [theme, setTheme] = useState<ThemeMode>(() => (localStorage.getItem(THEME_STORAGE_KEY) === 'light' ? 'light' : 'dark'));

  const [offers, setOffers] = useState<OddsOffer[]>([]);
  const [myBets, setMyBets] = useState<UserBet[]>([]);
  const [pendingBets, setPendingBets] = useState<UserBet[]>([]);
  const [selectedBet, setSelectedBet] = useState<SelectedBet | null>(null);
  const [stake, setStake] = useState<string>('100');

  const [syncSettings, setSyncSettings] = useState<OddsSyncSettings | null>(null);
  const [adminSportKey, setAdminSportKey] = useState('upcoming');
  const [adminAutoRefresh, setAdminAutoRefresh] = useState(false);
  const [adminIntervalSeconds, setAdminIntervalSeconds] = useState('60');

  const [section, setSection] = useState<Section>('bets');
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const [loginEmail, setLoginEmail] = useState('');
  const [loginPassword, setLoginPassword] = useState('');
  const [registerName, setRegisterName] = useState('');
  const [registerEmail, setRegisterEmail] = useState('');
  const [registerPassword, setRegisterPassword] = useState('');

  const [depositAmount, setDepositAmount] = useState('');
  const [depositTransactionId, setDepositTransactionId] = useState('');
  const [deposits, setDeposits] = useState<DepositRequest[]>([]);

  const [withdrawAmount, setWithdrawAmount] = useState('');
  const [withdrawals, setWithdrawals] = useState<WithdrawalRequest[]>([]);
  const [walletSummary, setWalletSummary] = useState<WalletSummary | null>(null);

  const [settingsDisplayName, setSettingsDisplayName] = useState('');
  const [settingsMethod, setSettingsMethod] = useState<PaymentMethod>('CUP');
  const [settingsAccount, setSettingsAccount] = useState('');

  const isAdmin = useMemo(() => roles.includes('Admin'), [roles]);
  const topOffers = useMemo(() => offers.slice(0, 4), [offers]);

  const totalDeposited = useMemo(() => walletSummary?.totalDeposited ?? 0, [walletSummary]);
  const pendingDeposits = useMemo(() => walletSummary?.pendingDeposits ?? 0, [walletSummary]);
  const totalWithdrawn = useMemo(() => walletSummary?.totalWithdrawn ?? 0, [walletSummary]);
  const pendingWithdrawals = useMemo(() => walletSummary?.pendingWithdrawals ?? 0, [walletSummary]);

  useEffect(() => {
    setAuthToken(token);
  }, [token]);

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme;
    localStorage.setItem(THEME_STORAGE_KEY, theme);
  }, [theme]);

  useEffect(() => {
    if (token) {
      void refreshData();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  useEffect(() => {
    if (!isAdmin && section === 'admin') {
      setSection('bets');
    }
  }, [isAdmin, section]);

  useEffect(() => {
    if (user) {
      setSettingsDisplayName(user.displayName);
    }
  }, [user]);

  const saveSession = (auth: AuthResponse) => {
    setToken(auth.token);
    setRoles(auth.roles);
    setUser(auth.user);
    localStorage.setItem('token', auth.token);
    localStorage.setItem('roles', JSON.stringify(auth.roles));
    localStorage.setItem('user', JSON.stringify(auth.user));
    setMessage(`Bienvenido, ${auth.user.displayName}.`);
  };

  const clearSession = () => {
    setToken(null);
    setRoles([]);
    setUser(null);
    setOffers([]);
    setMyBets([]);
    setPendingBets([]);
    setSelectedBet(null);
    setSection('bets');
    setSidebarOpen(false);
    setProfileOpen(false);
    localStorage.removeItem('token');
    localStorage.removeItem('roles');
    localStorage.removeItem('user');
    setMessage('Sesión cerrada.');
  };

  async function refreshData() {
    setLoading(true);
    try {
      const [profileResult, offersResult, myBetsResult, summaryResult, depositsResult, withdrawalsResult, settingsResult] = await Promise.allSettled([
        authApi.me(),
        oddsApi.getOffers(),
        betsApi.myBets(),
        walletApi.summary(),
        walletApi.deposits(),
        walletApi.withdrawals(),
        walletApi.getWithdrawalSettings(),
      ]);

      const coreFailures: string[] = [];

      if (profileResult.status === 'fulfilled') {
        setUser(profileResult.value);
        localStorage.setItem('user', JSON.stringify(profileResult.value));
      } else {
        coreFailures.push('perfil');
      }

      if (offersResult.status === 'fulfilled') {
        setOffers(offersResult.value);
      } else {
        coreFailures.push('ofertas');
      }

      if (myBetsResult.status === 'fulfilled') {
        setMyBets(myBetsResult.value);
      } else {
        coreFailures.push('mis apuestas');
      }

      if (summaryResult.status === 'fulfilled') {
        setWalletSummary(summaryResult.value);
      }

      if (depositsResult.status === 'fulfilled') {
        setDeposits(depositsResult.value);
      }

      if (withdrawalsResult.status === 'fulfilled') {
        setWithdrawals(withdrawalsResult.value);
      }

      if (settingsResult.status === 'fulfilled') {
        setSettingsMethod(fromApiMethod(settingsResult.value.method));
        setSettingsAccount(settingsResult.value.account ?? '');
      }

      if (isAdmin) {
        const [pendingResult, syncResult] = await Promise.allSettled([betsApi.pendingBets(), oddsApi.getSyncSettings()]);
        if (pendingResult.status === 'fulfilled') {
          setPendingBets(pendingResult.value);
        } else {
          setPendingBets([]);
        }

        if (syncResult.status === 'fulfilled') {
          setSyncSettings(syncResult.value);
          setAdminSportKey(syncResult.value.currentSportKey || 'upcoming');
          setAdminAutoRefresh(syncResult.value.autoRefreshEnabled);
          setAdminIntervalSeconds(String(syncResult.value.refreshIntervalSeconds));
        }
      } else {
        setPendingBets([]);
      }

      if (coreFailures.length > 0) {
        setMessage(`Error parcial cargando: ${coreFailures.join(', ')}.`);
      } else if (
        summaryResult.status === 'rejected' ||
        depositsResult.status === 'rejected' ||
        withdrawalsResult.status === 'rejected' ||
        settingsResult.status === 'rejected'
      ) {
        setMessage('Datos wallet no disponibles temporalmente.');
      }
    } finally {
      setLoading(false);
    }
  }

  const handleLogin = async () => {
    try {
      setLoading(true);
      const auth = await authApi.login({ email: loginEmail, password: loginPassword });
      saveSession(auth);
      setLoginPassword('');
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo iniciar sesión.');
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async () => {
    try {
      setLoading(true);
      const auth = await authApi.register({
        displayName: registerName,
        email: registerEmail,
        password: registerPassword,
      });
      saveSession(auth);
      setRegisterPassword('');
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo registrar.');
    } finally {
      setLoading(false);
    }
  };

  const handlePlaceBet = async () => {
    if (!selectedBet) {
      setMessage('Selecciona una cuota primero.');
      return;
    }

    const amount = Number(stake);
    if (Number.isNaN(amount) || amount <= 0) {
      setMessage('El stake debe ser mayor que 0.');
      return;
    }

    try {
      setLoading(true);
      const market = selectedBet.offer.bookmaker.markets[0];
      await betsApi.placeBet({
        eventId: selectedBet.offer.eventId,
        sportKey: selectedBet.offer.sportKey,
        homeTeam: selectedBet.offer.homeTeam,
        awayTeam: selectedBet.offer.awayTeam,
        commenceTime: selectedBet.offer.commenceTime,
        bookmakerKey: selectedBet.offer.bookmaker.key,
        bookmakerTitle: selectedBet.offer.bookmaker.title,
        marketKey: market?.key ?? 'h2h',
        selectedOutcomeName: selectedBet.outcome.name,
        selectedOutcomePrice: selectedBet.outcome.price,
        stake: amount,
      });

      setMessage('Apuesta creada correctamente.');
      setSelectedBet(null);
      setStake('100');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo crear la apuesta.');
    } finally {
      setLoading(false);
    }
  };

  const handleResolve = async (betId: string, isWon: boolean) => {
    try {
      setLoading(true);
      await betsApi.resolveBet(betId, isWon);
      setMessage(`Apuesta ${isWon ? 'ganada' : 'perdida'} actualizada.`);
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo resolver la apuesta.');
    } finally {
      setLoading(false);
    }
  };

  const handleAdminRefresh = async () => {
    try {
      setLoading(true);
      await oddsApi.refreshNow(adminSportKey || 'upcoming');
      setMessage('Ofertas sincronizadas desde The Odds API.');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo refrescar odds.');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveSyncSettings = async () => {
    try {
      setLoading(true);
      await oddsApi.updateSyncSettings({
        autoRefreshEnabled: adminAutoRefresh,
        refreshIntervalSeconds: Number(adminIntervalSeconds) || 60,
        sportKey: adminSportKey || 'upcoming',
      });
      setMessage('Configuración de sync guardada.');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo guardar la configuración.');
    } finally {
      setLoading(false);
    }
  };

  const toggleTheme = () => setTheme((current) => (current === 'dark' ? 'light' : 'dark'));

  const createDeposit = async () => {
    const amount = Number(depositAmount);
    if (Number.isNaN(amount) || amount <= 0) {
      setMessage('Monto de deposito invalido.');
      return;
    }
    if (!depositTransactionId.trim()) {
      setMessage('Debes indicar id de transacción.');
      return;
    }

    try {
      setLoading(true);
      await walletApi.createDeposit({
        amount,
        transactionId: depositTransactionId.trim(),
      });
      setDepositAmount('');
      setDepositTransactionId('');
      setMessage('Solicitud de deposito creada.');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo crear el deposito.');
    } finally {
      setLoading(false);
    }
  };

  const createWithdrawal = async () => {
    const amount = Number(withdrawAmount);
    if (Number.isNaN(amount) || amount <= 0) {
      setMessage('Monto de retiro invalido.');
      return;
    }
    if (!settingsAccount.trim()) {
      setMessage('Primero define la cuenta de retiro en settings.');
      return;
    }

    try {
      setLoading(true);
      await walletApi.createWithdrawal({ amount });
      setWithdrawAmount('');
      setMessage('Solicitud de retiro creada.');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo crear el retiro.');
    } finally {
      setLoading(false);
    }
  };

  const saveSettings = async () => {
    if (!settingsDisplayName.trim()) {
      setMessage('Nombre invalido.');
      return;
    }
    if (!settingsAccount.trim()) {
      setMessage('Cuenta de retiro requerida.');
      return;
    }

    try {
      setLoading(true);
      await walletApi.updateWithdrawalSettings({
        method: toApiMethod(settingsMethod),
        account: settingsAccount.trim(),
        displayName: settingsDisplayName.trim(),
      });
      setMessage('Settings guardados.');
      await refreshData();
    } catch (error: any) {
      setMessage(error?.response?.data?.message ?? 'No se pudo guardar settings.');
    } finally {
      setLoading(false);
    }
  };

  const selectSection = (next: Section) => {
    setSection(next);
    setSidebarOpen(false);
  };

  if (!token) {
    return (
      <div className="auth-shell-stitch">
        <section className="auth-visual-panel">
          <div className="auth-brand">MIDNIGHT ARENA</div>
          <h1>Sportsbook Premium</h1>
          <p>Accede a cuotas en vivo, historial, depósito y retiros en una experiencia unificada.</p>
        </section>

        <section className="auth-form-panel">
          <div className="auth-tabs">
            <button type="button" className={authTab === 'login' ? 'active' : ''} onClick={() => setAuthTab('login')}>Entrar</button>
            <button type="button" className={authTab === 'register' ? 'active' : ''} onClick={() => setAuthTab('register')}>Registro</button>
          </div>

          {authTab === 'login' ? (
            <form className="auth-form" onSubmit={(event) => { event.preventDefault(); void handleLogin(); }}>
              <label>
                <span>Correo</span>
                <input type="email" value={loginEmail} onChange={(event) => setLoginEmail(event.target.value)} />
              </label>
              <label>
                <span>Contraseña</span>
                <input type="password" value={loginPassword} onChange={(event) => setLoginPassword(event.target.value)} />
              </label>
              <button type="submit" className="cta" disabled={loading}>{loading ? 'Entrando...' : 'Entrar'}</button>
            </form>
          ) : (
            <form className="auth-form" onSubmit={(event) => { event.preventDefault(); void handleRegister(); }}>
              <label>
                <span>Nombre</span>
                <input type="text" value={registerName} onChange={(event) => setRegisterName(event.target.value)} />
              </label>
              <label>
                <span>Correo</span>
                <input type="email" value={registerEmail} onChange={(event) => setRegisterEmail(event.target.value)} />
              </label>
              <label>
                <span>Contraseña</span>
                <input type="password" value={registerPassword} onChange={(event) => setRegisterPassword(event.target.value)} />
              </label>
              <button type="submit" className="cta" disabled={loading}>{loading ? 'Creando...' : 'Crear cuenta'}</button>
            </form>
          )}

          {message && <p className="auth-message">{message}</p>}
        </section>
      </div>
    );
  }

  return (
    <div className="stitch-app">
      <header className="stitch-topbar">
        <div className="topbar-left">
          <button type="button" className="mobile-menu" onClick={() => setSidebarOpen((open) => !open)}>☰</button>
        </div>

        <div className="topbar-center">
          <span className="brand">MIDNIGHT ARENA</span>
        </div>

        <div className="topbar-right">
          <div className="balance-chip">
            <span>Balance</span>
            <strong>{formatCurrency(user?.balance ?? 0)}</strong>
          </div>

          <div className="profile-wrapper">
            <button type="button" className="avatar" onClick={() => setProfileOpen((open) => !open)}>
              {user?.displayName?.slice(0, 1).toUpperCase() ?? 'U'}
            </button>
            {profileOpen && (
              <div className="profile-popover">
                <p>{user?.email}</p>
                <div className="theme-toggle-item">
                  <span className="theme-toggle-label">
                    <Icon name={theme === 'dark' ? 'moon' : 'sun'} />
                    <span>{theme === 'dark' ? 'Modo Oscuro' : 'Modo Claro'}</span>
                  </span>
                  <button type="button" className={theme === 'dark' ? 'theme-toggle dark' : 'theme-toggle light'} onClick={toggleTheme} aria-label="Cambiar tema">
                    <span className="theme-toggle-thumb">
                      <Icon name={theme === 'dark' ? 'moon' : 'sun'} />
                    </span>
                  </button>
                </div>
                <button type="button" onClick={() => { selectSection('settings'); setProfileOpen(false); }}>
                  <Icon name="settings" />
                  <span>Settings</span>
                </button>
                <button type="button" className="danger" onClick={clearSession}>
                  <Icon name="logout" />
                  <span>Logout</span>
                </button>
              </div>
            )}
          </div>
        </div>
      </header>

      {profileOpen && <button type="button" className="overlay" onClick={() => setProfileOpen(false)} aria-label="Cerrar menú" />}
      {sidebarOpen && <button type="button" className="overlay sidebar-overlay" onClick={() => setSidebarOpen(false)} aria-label="Cerrar menú lateral" />}

      {message && <div className="notice-stitch">{message}</div>}

      <div className="stitch-layout">
        <aside className={sidebarOpen ? 'stitch-sidebar open' : 'stitch-sidebar'}>
          <div className="sidebar-account">
            <div className="badge">VIP</div>
            <div>
              <p>{user?.displayName}</p>
              <small>{isAdmin ? 'Admin' : 'Usuario'}</small>
            </div>
          </div>

          <nav className="sidebar-nav-stitch">
            <button type="button" className={section === 'bets' ? 'active' : ''} onClick={() => selectSection('bets')}><Icon name="bets" /><span>My Bets</span></button>
            <button type="button" className={section === 'deposits' ? 'active' : ''} onClick={() => selectSection('deposits')}><Icon name="deposits" /><span>Deposits</span></button>
            <button type="button" className={section === 'withdrawals' ? 'active' : ''} onClick={() => selectSection('withdrawals')}><Icon name="withdrawals" /><span>Withdrawals</span></button>
            <button type="button" className={section === 'settings' ? 'active' : ''} onClick={() => selectSection('settings')}><Icon name="settings" /><span>Settings</span></button>
            {isAdmin && <button type="button" className={section === 'admin' ? 'active' : ''} onClick={() => selectSection('admin')}><Icon name="admin" /><span>Admin Panel</span></button>}
          </nav>
        </aside>

        <main className="stitch-content">
          {section === 'bets' && (
            <section className="bets-page">
              <div className="hero-stitch">
                <h1>CHAMPIONS LEAGUE</h1>
                <p>{syncSettings?.currentSportKey ?? 'upcoming'} · Live board</p>
              </div>

              <div className="bets-grid-layout">
                <div className="live-board">
                  <header>
                    <h2>Live Now</h2>
                    <span>{offers.length} eventos</span>
                  </header>

                  {offers.length === 0 ? (
                    <div className="empty-card">No hay ofertas disponibles.</div>
                  ) : (
                    offers.map((offer) => {
                      const market = offer.bookmaker.markets.find((m) => m.key === 'h2h') ?? offer.bookmaker.markets[0];
                      return (
                        <article key={offer.eventId} className="event-card">
                          <div className="event-meta">
                            <span>{formatSportLabel(offer.sportKey)}</span>
                            <span>{formatDateTime(offer.commenceTime)}</span>
                          </div>
                          <h3>{offer.homeTeam} vs {offer.awayTeam}</h3>
                          <div className="odds-row">
                            {market?.outcomes.map((outcome) => {
                              const selected = selectedBet?.offer.eventId === offer.eventId && selectedBet.outcome.name === outcome.name;
                              return (
                                <button
                                  key={`${offer.eventId}-${outcome.name}`}
                                  type="button"
                                  className={selected ? 'odd selected' : 'odd'}
                                  onClick={() => setSelectedBet({ offer, outcome })}
                                >
                                  <span>{outcome.name}</span>
                                  <strong>{outcome.price.toFixed(2)}</strong>
                                </button>
                              );
                            })}
                          </div>
                        </article>
                      );
                    })
                  )}
                </div>

                <aside className="slip-stitch">
                  <div className="slip-head">
                    <h3>BET SLIP</h3>
                    <span>{selectedBet ? '1 ACTIVE' : '0 ACTIVE'}</span>
                  </div>

                  {selectedBet ? (
                    <div className="slip-body">
                      <p>{selectedBet.offer.homeTeam} vs {selectedBet.offer.awayTeam}</p>
                      <small>{selectedBet.outcome.name} · {selectedBet.outcome.price.toFixed(2)}</small>
                      <label>
                        <span>Stake</span>
                        <input type="number" min="1" step="0.01" value={stake} onChange={(event) => setStake(event.target.value)} />
                      </label>
                      <button type="button" className="cta" onClick={handlePlaceBet} disabled={loading}>PLACE BET</button>
                    </div>
                  ) : (
                    <div className="empty-card">Selecciona una cuota para apostar.</div>
                  )}

                  <section className="my-bets-mini">
                    <header>
                      <h4>Mis apuestas</h4>
                      <span>{myBets.length}</span>
                    </header>
                    <div className="mini-list">
                      {myBets.length === 0 ? (
                        <div className="empty-card">No tienes apuestas.</div>
                      ) : (
                        myBets.map((bet) => (
                          <article key={bet.id} className={`mini-item ${bet.status.toLowerCase()}`}>
                            <div>
                              <strong>{bet.homeTeam} vs {bet.awayTeam}</strong>
                              <small>{formatSportLabel(bet.sportKey)}</small>
                            </div>
                            <span className={`status ${bet.status.toLowerCase()}`}>{betStatusLabel[bet.status]}</span>
                            <div className="mini-meta">
                              <span>{bet.selectedOutcomeName}</span>
                              <span>{formatCurrency(bet.stake)}</span>
                            </div>
                          </article>
                        ))
                      )}
                    </div>
                  </section>
                </aside>
              </div>

              {topOffers.length > 0 && (
                <section className="promo-bento">
                  {topOffers.map((offer) => (
                    <article key={`promo-${offer.eventId}`}>
                      <h4>{offer.homeTeam} vs {offer.awayTeam}</h4>
                      <p>{formatDateTime(offer.commenceTime)}</p>
                    </article>
                  ))}
                </section>
              )}
            </section>
          )}

          {section === 'deposits' && (
            <section className="wallet-page">
              <header className="page-head">
                <h1>Deposit Funds</h1>
                <p>Add balance to your wallet instantly.</p>
              </header>

              <div className="wallet-grid">
                <div className="wallet-main">
                  <div className="stats-row">
                    <article>
                      <span>Total Deposited</span>
                      <strong>{formatCurrency(totalDeposited)}</strong>
                    </article>
                    <article>
                      <span>Pending Deposits</span>
                      <strong>{formatCurrency(pendingDeposits)}</strong>
                    </article>
                  </div>

                  <section className="history-card">
                    <header>
                      <h2>Recent Transactions</h2>
                      <span>{deposits.length}</span>
                    </header>
                    <div className="table-wrap">
                      <table>
                        <thead>
                          <tr>
                            <th>Transaction ID</th>
                            <th>Method</th>
                            <th>Date</th>
                            <th>Status</th>
                            <th>Amount</th>
                          </tr>
                        </thead>
                        <tbody>
                          {deposits.length === 0 ? (
                            <tr><td colSpan={5}>No hay transacciones.</td></tr>
                          ) : (
                            deposits.map((item) => (
                              <tr key={item.id}>
                                <td data-label="Transaction ID">{item.transactionId}</td>
                                <td data-label="Method">Manual Transfer</td>
                                <td data-label="Date">{formatDateTime(item.createdAt)}</td>
                                <td data-label="Status"><span className={`status ${item.status.toLowerCase()}`}>{requestStatusLabel(item.status)}</span></td>
                                <td data-label="Amount">{formatCurrency(item.amount)}</td>
                              </tr>
                            ))
                          )}
                        </tbody>
                      </table>
                    </div>
                  </section>
                </div>

                <aside className="wallet-aside">
                  <h2>New Request</h2>
                  <form className="wallet-form" onSubmit={(event) => { event.preventDefault(); void createDeposit(); }}>
                    <label>
                      <span>Deposit Amount ($)</span>
                      <input type="number" min="1" step="0.01" value={depositAmount} onChange={(event) => setDepositAmount(event.target.value)} />
                    </label>
                    <label>
                      <span>Transaction ID / Reference</span>
                      <input type="text" value={depositTransactionId} onChange={(event) => setDepositTransactionId(event.target.value)} />
                    </label>
                    <button type="submit" className="cta" disabled={loading}>Submit Deposit Request</button>
                  </form>
                </aside>
              </div>
            </section>
          )}

          {section === 'withdrawals' && (
            <section className="wallet-page">
              <header className="page-head">
                <h1>Withdrawals</h1>
                <p>Manage your winnings and transfer status.</p>
              </header>

              <div className="stats-row three">
                <article>
                  <span>Total Withdrawn</span>
                  <strong>{formatCurrency(totalWithdrawn)}</strong>
                </article>
                <article>
                  <span>Pending</span>
                  <strong>{formatCurrency(pendingWithdrawals)}</strong>
                </article>
                <article className="action-card">
                  <span>Request New</span>
                  <button type="button" onClick={() => setSection('settings')}>Go Settings</button>
                </article>
              </div>

              <div className="wallet-grid single">
                <section className="history-card">
                  <header>
                    <h2>Detailed History</h2>
                    <span>{withdrawals.length}</span>
                  </header>
                  <div className="table-wrap">
                    <table>
                      <thead>
                        <tr>
                          <th>Transaction ID</th>
                          <th>Date</th>
                          <th>Method</th>
                          <th>Amount</th>
                          <th>Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {withdrawals.length === 0 ? (
                          <tr><td colSpan={5}>No hay retiros.</td></tr>
                        ) : (
                          withdrawals.map((item) => (
                            <tr key={item.id}>
                              <td data-label="Transaction ID">{item.id.slice(0, 8).toUpperCase()}</td>
                              <td data-label="Date">{formatDateTime(item.createdAt)}</td>
                              <td data-label="Method">{item.method} · {item.account}</td>
                              <td data-label="Amount">{formatCurrency(item.amount)}</td>
                              <td data-label="Status"><span className={`status ${item.status.toLowerCase()}`}>{requestStatusLabel(item.status)}</span></td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                </section>

                <aside className="wallet-aside">
                  <h2>New Withdrawal</h2>
                  <form className="wallet-form" onSubmit={(event) => { event.preventDefault(); void createWithdrawal(); }}>
                    <label>
                      <span>Amount ($)</span>
                      <input type="number" min="1" step="0.01" value={withdrawAmount} onChange={(event) => setWithdrawAmount(event.target.value)} />
                    </label>
                    <label>
                      <span>Method</span>
                      <input type="text" value={settingsMethod} disabled />
                    </label>
                    <label>
                      <span>Account</span>
                      <input type="text" value={settingsAccount} disabled />
                    </label>
                    <button type="submit" className="cta" disabled={loading}>Withdraw Funds</button>
                  </form>
                </aside>
              </div>
            </section>
          )}

          {section === 'settings' && (
            <section className="settings-page">
              <header className="page-head">
                <h1>Settings</h1>
                <p>Perfil y configuración de retiro.</p>
              </header>

              <div className="settings-card">
                <form className="wallet-form" onSubmit={(event) => { event.preventDefault(); void saveSettings(); }}>
                  <label>
                    <span>Nombre</span>
                    <input type="text" value={settingsDisplayName} onChange={(event) => setSettingsDisplayName(event.target.value)} />
                  </label>
                  <label>
                    <span>Tipo de retiro</span>
                    <select value={settingsMethod} onChange={(event) => setSettingsMethod(event.target.value as PaymentMethod)}>
                      <option value="CUP">CUP</option>
                      <option value="MLC">MLC</option>
                      <option value="QvaPay">QvaPay</option>
                    </select>
                  </label>
                  <label>
                    <span>Cuenta de retiro</span>
                    <input type="text" value={settingsAccount} onChange={(event) => setSettingsAccount(event.target.value)} />
                  </label>
                  <button type="submit" className="cta" disabled={loading}>Guardar Settings</button>
                </form>
              </div>
            </section>
          )}

          {isAdmin && section === 'admin' && (
            <section className="admin-page">
              <header className="page-head admin-headline">
                <div>
                  <h1>Pending Bets</h1>
                  <p>Active Monitoring: {pendingBets.length} slips awaiting resolution</p>
                </div>
                <div className="admin-controls-inline">
                  <input value={adminSportKey} onChange={(event) => setAdminSportKey(event.target.value)} placeholder="upcoming" />
                  <input type="number" min="15" step="1" value={adminIntervalSeconds} onChange={(event) => setAdminIntervalSeconds(event.target.value)} />
                  <label className="inline-switch">
                    <span>Auto</span>
                    <input type="checkbox" checked={adminAutoRefresh} onChange={() => setAdminAutoRefresh((v) => !v)} />
                  </label>
                  <button type="button" onClick={handleSaveSyncSettings}>Save</button>
                  <button type="button" onClick={handleAdminRefresh}>Refresh</button>
                </div>
              </header>

              <section className="history-card">
                <div className="table-wrap">
                  <table>
                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>User</th>
                        <th>Event</th>
                        <th>Selection</th>
                        <th>Stake</th>
                        <th>Odds</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pendingBets.length === 0 ? (
                        <tr><td colSpan={7}>No hay apuestas pendientes.</td></tr>
                      ) : (
                        pendingBets.map((bet) => (
                          <tr key={bet.id}>
                            <td data-label="ID">{bet.id.slice(0, 8).toUpperCase()}</td>
                            <td data-label="User">{bet.userDisplayName}</td>
                            <td data-label="Event">{bet.homeTeam} vs {bet.awayTeam}</td>
                            <td data-label="Selection">{bet.selectedOutcomeName}</td>
                            <td data-label="Stake">{formatCurrency(bet.stake)}</td>
                            <td data-label="Odds">{bet.selectedOutcomePrice.toFixed(2)}</td>
                            <td data-label="Actions">
                              <div className="admin-actions">
                                <button type="button" className="win" onClick={() => handleResolve(bet.id, true)}>Won</button>
                                <button type="button" className="lose" onClick={() => handleResolve(bet.id, false)}>Lost</button>
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              </section>

              <section className="admin-insights">
                <article>
                  <span>Payout Liability</span>
                  <strong>{formatCurrency(pendingBets.reduce((sum, bet) => sum + bet.stake * bet.selectedOutcomePrice, 0))}</strong>
                </article>
                <article>
                  <span>Last Sync</span>
                  <strong>{syncSettings?.lastRefreshAt ? formatDateTime(syncSettings.lastRefreshAt) : 'Never'}</strong>
                </article>
              </section>
            </section>
          )}
        </main>
      </div>
    </div>
  );
}
