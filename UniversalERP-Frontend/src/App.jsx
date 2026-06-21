import { useEffect, useMemo, useRef, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/Login";
import RegisterPage from "./pages/Register";
import DashboardPage from "./pages/Dashboard";
import ProductsPage from "./pages/Products";
import CustomersPage from "./pages/Customers";
import SalesPage from "./pages/Sales";
import ServiceOrdersPage from "./pages/ServiceOrders";
import ProjectTasksPage from "./pages/ProjectTasks";
import ReportsPage from "./pages/Reports";
import AuditLogsPage from "./pages/AuditLogs";
import ProfilePage from "./pages/Profile";
import TenantSettingsPage from "./pages/TenantSettings";
import Layout from "./components/Layout";
import NotificationToasts from "./components/NotificationToasts";
import { decodeJwt } from "./api/auth";
import { startSignalRConnection, stopSignalRConnection } from "./api/signalr";

let toastIdCounter = 0;

export default function App() {
  const [token, setToken] = useState(() => localStorage.getItem("erp_token") || "");
  const [refreshTokenValue, setRefreshTokenValue] = useState(
    () => localStorage.getItem("erp_refresh_token") || ""
  );
  const [authView, setAuthView] = useState("login"); // "login" | "register"
  const [notifications, setNotifications] = useState([]);
  const dismissTimers = useRef({});

  const payload = useMemo(() => decodeJwt(token), [token]);

  const role =
    payload?.role ||
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
    "";
  const fullName = payload?.FullName || payload?.fullName || "";
  const email =
    payload?.email ||
    payload?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
    "";
  const tenantId = payload?.TenantId ? Number(payload.TenantId) : 0;

  useEffect(() => {
    if (token) localStorage.setItem("erp_token", token);
    else localStorage.removeItem("erp_token");
  }, [token]);

  useEffect(() => {
    if (refreshTokenValue) localStorage.setItem("erp_refresh_token", refreshTokenValue);
    else localStorage.removeItem("erp_refresh_token");
  }, [refreshTokenValue]);

  // SignalR bağlantısı — token varken kur, yokken kapat
  useEffect(() => {
    if (!token) return;

    startSignalRConnection(token, (notification) => {
      const id = ++toastIdCounter;
      setNotifications((prev) => [...prev, { ...notification, id }]);

      dismissTimers.current[id] = setTimeout(() => {
        setNotifications((prev) => prev.filter((n) => n.id !== id));
      }, 6000);
    });

    return () => {
      stopSignalRConnection();
      Object.values(dismissTimers.current).forEach(clearTimeout);
      dismissTimers.current = {};
    };
  }, [token]);

  function dismissNotification(id) {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
    if (dismissTimers.current[id]) {
      clearTimeout(dismissTimers.current[id]);
      delete dismissTimers.current[id];
    }
  }

  function onLogin(authResponse) {
    setToken(authResponse.accessToken);
    setRefreshTokenValue(authResponse.refreshToken);
  }

  function onLogout() {
    setToken("");
    setRefreshTokenValue("");
    localStorage.removeItem("erp_token");
    localStorage.removeItem("erp_refresh_token");
  }

  if (!token) {
    if (authView === "register") {
      return (
        <RegisterPage
          onRegistered={() => setAuthView("login")}
          onBackToLogin={() => setAuthView("login")}
        />
      );
    }
    return <LoginPage onLogin={onLogin} onShowRegister={() => setAuthView("register")} />;
  }

  return (
    <>
      <NotificationToasts notifications={notifications} onDismiss={dismissNotification} />
      <BrowserRouter>
        <Routes>
          <Route
            element={
              <Layout
                onLogout={onLogout}
                role={role}
                fullName={fullName}
                email={email}
                tenantId={tenantId}
              />
            }
          >
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/products" element={<ProductsPage role={role} />} />
            <Route path="/customers" element={<CustomersPage role={role} />} />
            <Route path="/sales" element={<SalesPage role={role} />} />
            <Route path="/service-orders" element={<ServiceOrdersPage role={role} />} />
            <Route path="/tasks" element={<ProjectTasksPage role={role} />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/tenant-settings" element={<TenantSettingsPage />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  );
}
