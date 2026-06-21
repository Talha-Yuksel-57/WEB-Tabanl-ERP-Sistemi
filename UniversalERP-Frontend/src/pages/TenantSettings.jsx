import { useEffect, useState } from "react";
import { getTenantSettings, updateTenantSettings } from "../api/tenants";

export default function TenantSettingsPage() {
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);
  const [msg, setMsg] = useState(null);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({ name: "", address: "", taxNumber: "", industry: "" });
  const [planType, setPlanType] = useState("");

  useEffect(() => {
    getTenantSettings()
      .then((data) => {
        setForm({
          name: data.name || "",
          address: data.address || "",
          taxNumber: data.taxNumber || "",
          industry: data.industry || "",
        });
        setPlanType(data.planType || "");
      })
      .catch((ex) => setErr(ex.message || "Firma ayarları yüklenemedi"))
      .finally(() => setLoading(false));
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    setMsg(null);
    try {
      await updateTenantSettings(form);
      setMsg("Firma ayarları güncellendi.");
    } catch (ex) {
      setErr(ex.message || "Güncelleme başarısız");
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="card">Yükleniyor...</div>;

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Firma Ayarları</h2>

      <div className="card">
        {planType && (
          <p style={{ color: "var(--muted)", fontSize: 13, marginTop: -6 }}>
            Mevcut Plan: <strong>{planType}</strong>
          </p>
        )}

        {err && <div className="error" style={{ marginBottom: 12 }}>{err}</div>}
        {msg && (
          <div
            className="card"
            style={{ background: "#f0fdf4", border: "1px solid #16a34a", color: "#14532d", marginBottom: 12 }}
          >
            {msg}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="field">
              <label>Firma Adı</label>
              <input
                required
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Vergi Numarası</label>
              <input
                value={form.taxNumber}
                onChange={(e) => setForm({ ...form, taxNumber: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Sektör</label>
              <input
                value={form.industry}
                onChange={(e) => setForm({ ...form, industry: e.target.value })}
              />
            </div>
          </div>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>Adres</label>
            <input
              value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })}
              style={{ width: "100%" }}
            />
          </div>
          <button className="btn" disabled={saving}>
            {saving ? "Kaydediliyor..." : "Kaydet"}
          </button>
        </form>
      </div>
    </div>
  );
}
