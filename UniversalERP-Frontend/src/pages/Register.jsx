import { useEffect, useState } from "react";
import { register } from "../api/auth";
import { getTenants } from "../api/tenants";

export default function RegisterPage({ onRegistered, onBackToLogin }) {
  const [tenants, setTenants] = useState([]);
  const [loadingTenants, setLoadingTenants] = useState(true);
  const [err, setErr] = useState(null);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    tenantId: "",
    role: "Customer",
  });

  useEffect(() => {
    getTenants()
      .then((data) => setTenants(data || []))
      .catch((ex) => setErr(ex.message || "Firmalar yüklenemedi"))
      .finally(() => setLoadingTenants(false));
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      await register({
        fullName: form.fullName,
        email: form.email,
        password: form.password,
        tenantId: Number(form.tenantId),
        role: form.role,
      });
      onRegistered?.();
    } catch (ex) {
      setErr(ex.message || "Kayıt başarısız");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: 460, margin: "60px auto" }}>
        <h2 style={{ marginTop: 0 }}>Hesap Oluştur</h2>

        {err && <div className="error" style={{ marginBottom: 12 }}>{err}</div>}

        <form onSubmit={handleSubmit}>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>Ad Soyad</label>
            <input
              required
              value={form.fullName}
              onChange={(e) => setForm({ ...form, fullName: e.target.value })}
              style={{ width: "100%" }}
            />
          </div>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>E-posta</label>
            <input
              required
              type="email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              style={{ width: "100%" }}
            />
          </div>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>Şifre</label>
            <input
              required
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              style={{ width: "100%" }}
            />
          </div>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>Firma</label>
            <select
              required
              disabled={loadingTenants}
              value={form.tenantId}
              onChange={(e) => setForm({ ...form, tenantId: e.target.value })}
              style={{ width: "100%" }}
            >
              <option value="">{loadingTenants ? "Yükleniyor..." : "Seçiniz"}</option>
              {tenants.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </div>
          <div className="field" style={{ marginBottom: 18 }}>
            <label>Rol</label>
            <select
              value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
              style={{ width: "100%" }}
            >
              <option value="Customer">Müşteri</option>
              <option value="Employee">Personel</option>
              <option value="Cashier">Kasiyer</option>
              <option value="Technician">Teknisyen</option>
              <option value="Manager">Müdür</option>
              <option value="TenantAdmin">Firma Yöneticisi</option>
            </select>
          </div>

          <button className="btn" disabled={saving} style={{ width: "100%", marginBottom: 10 }}>
            {saving ? "Kayıt yapılıyor..." : "Hesap Oluştur"}
          </button>
          <button
            type="button"
            className="btn secondary"
            style={{ width: "100%" }}
            onClick={onBackToLogin}
          >
            Girişe Geri Dön
          </button>
        </form>
      </div>
    </div>
  );
}
