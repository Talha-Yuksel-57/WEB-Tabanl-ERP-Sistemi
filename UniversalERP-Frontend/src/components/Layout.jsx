import { useEffect, useState } from "react";
import { Outlet, NavLink } from "react-router-dom";
import { hasRole, CAN_EDIT_TENANT_SETTINGS } from "../utils/permissions";

// Sisteme giriş yapan kullanıcının rolüne göre (Role-Based Access Control) 
// sol/üst menüde göreceği sayfaları belirleyen statik eşleme nesnesi (Dictionary)
const MENU_BY_ROLE = {
  SuperAdmin: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/products", label: "Ürünler" },
    { to: "/customers", label: "Müşteriler" },
    { to: "/sales", label: "Satış" },
    { to: "/service-orders", label: "Servis" },
    { to: "/tasks", label: "Görevler" },
    { to: "/reports", label: "Raporlar" },
    { to: "/audit-logs", label: "İşlem Geçmişi" },
  ],
  TenantAdmin: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/products", label: "Ürünler" },
    { to: "/customers", label: "Müşteriler" },
    { to: "/sales", label: "Satış" },
    { to: "/service-orders", label: "Servis" },
    { to: "/tasks", label: "Görevler" },
    { to: "/reports", label: "Raporlar" },
    { to: "/audit-logs", label: "İşlem Geçmişi" },
  ],
  Manager: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/products", label: "Ürünler" },
    { to: "/customers", label: "Müşteriler" },
    { to: "/sales", label: "Satış" },
    { to: "/service-orders", label: "Servis" },
    { to: "/tasks", label: "Görevler" },
    { to: "/reports", label: "Raporlar" },
  ],
  Cashier: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/sales", label: "Satış" },
    { to: "/products", label: "Ürünler" },
    { to: "/customers", label: "Müşteriler" },
  ],
  Technician: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/service-orders", label: "Servis" },
  ],
  Employee: [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/tasks", label: "Görevler" },
  ],
};

// Uygulamanın ana iskeletini ve yönlendirme (routing) şablonunu sunan Layout bileşeni
export default function Layout({ onLogout, role, fullName, email, tenantId }) {
  // Kullanıcının rolüne uygun menü listesini çeker, rol eşleşmezse varsayılan olarak sadece Dashboard'u gösterir
  const menu = MENU_BY_ROLE[role] || [{ to: "/dashboard", label: "Dashboard" }];
  
  // Kullanıcının firma ayarlarını düzenleme yetkisi (Permission) olup olmadığını kontrol eder
  const canEditTenant = hasRole(role, CAN_EDIT_TENANT_SETTINGS);

  // Tema durumunu (light/dark) tutan state. Tarayıcı yenilendiğinde kaybolmaması için ilk değeri localStorage'dan okur
  const [theme, setTheme] = useState(() => localStorage.getItem("erp_theme") || "light");

  // Tema state'i her değiştiğinde tetiklenen ve CSS sınıflarını/değişkenlerini güncelleyen Effect
  useEffect(() => {
    // HTML kök etiketine (<html>) "data-theme" niteliği ekleyerek CSS tarafında global tema değişimini tetikler
    document.documentElement.setAttribute("data-theme", theme);
    // Seçilen temayı kalıcı hale getirmek için localStorage'a kaydeder
    localStorage.setItem("erp_theme", theme);
  }, [theme]);

  // Açık ve koyu tema arasında geçiş yapan (toggle) buton fonksiyonu
  function toggleTheme() {
    setTheme((t) => (t === "light" ? "dark" : "light"));
  }

  return (
    <div className="container">
      {/* Üst Navigasyon Çubuğu (Navbar) */}
      <div className="nav">
        {/* Sol Taraf: Logo ve Kullanıcı Rolüne Göre Dinamik Oluşan Menü Linkleri */}
        <div className="row">
          <strong style={{ fontSize: 18 }}>UniversalERP</strong>
          {menu.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              // Tarayıcıdaki aktif URL ile eşleşen linke otomatik olarak "active" sınıfı (class) atar
              className={({ isActive }) => (isActive ? "active" : "")}
            >
              {item.label}
            </NavLink>
          ))}
        </div>

        {/* Sağ Taraf: Tema Değiştirici, Profil, Yetkiye Bağlı Firma Ayarları ve Kullanıcı Bilgileri */}
        <div className="row">
          {/* Tema Değiştirme Butonu */}
          <button className="theme-toggle" onClick={toggleTheme} title="Temayı değiştir">
            {theme === "light" ? "🌙 Koyu" : "☀️ Açık"}
          </button>
          
          {/* Sabit Profil Linki */}
          <NavLink to="/profile" className={({ isActive }) => (isActive ? "active" : "")}>
            Profil
          </NavLink>
          
          {/* Sadece 'canEditTenant' yetkisi 'true' olan kullanıcılara gösterilen dinamik bağlantı */}
          {canEditTenant && (
            <NavLink
              to="/tenant-settings"
              className={({ isActive }) => (isActive ? "active" : "")}
            >
              Firma Ayarları
            </NavLink>
          )}
          
          {/* Giriş yapan kullanıcının ad, e-posta, rol ve bağlı olduğu çoklu kiracı (Tenant/Firma) ID bilgisi */}
          <span style={{ color: "var(--muted)", fontSize: 13 }}>
            {fullName || email} {role ? `· ${role}` : ""} {tenantId ? `· Firma #${tenantId}` : ""}
          </span>
          
          {/* Oturumu kapatıp login ekranına yönlendiren çıkış butonu */}
          <button className="btn" onClick={onLogout}>
            Çıkış Yap
          </button>
        </div>
      </div>

      {/* React Router alt rotalarının (içerik sayfalarının) Layout içinde render edildiği dinamik alan */}
      <Outlet />
    </div>
  );
}