import { useEffect, useState } from "react";
import { getProfile, updateProfile, changePassword } from "../api/profile";

export default function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [form, setForm] = useState({ fullName: "", department: "" });
  const [savingProfile, setSavingProfile] = useState(false);
  const [profileMsg, setProfileMsg] = useState(null);

  const [pwForm, setPwForm] = useState({ currentPassword: "", newPassword: "", confirmPassword: "" });
  const [savingPw, setSavingPw] = useState(false);
  const [pwMsg, setPwMsg] = useState(null);
  const [pwErr, setPwErr] = useState(null);

  useEffect(() => {
    getProfile()
      .then((data) => {
        setProfile(data);
        setForm({ fullName: data.fullName, department: data.department || "" });
      })
      .catch((ex) => setErr(ex.message || "Profil yüklenemedi"))
      .finally(() => setLoading(false));
  }, []);

  async function handleProfileSubmit(e) {
    e.preventDefault();
    setSavingProfile(true);
    setProfileMsg(null);
    setErr(null);
    try {
      await updateProfile({ fullName: form.fullName, department: form.department });
      setProfileMsg("Profil güncellendi.");
    } catch (ex) {
      setErr(ex.message || "Güncelleme başarısız");
    } finally {
      setSavingProfile(false);
    }
  }

  async function handlePasswordSubmit(e) {
    e.preventDefault();
    setPwErr(null);
    setPwMsg(null);

    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwErr("Yeni şifreler eşleşmiyor.");
      return;
    }

    setSavingPw(true);
    try {
      await changePassword({
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword,
      });
      setPwMsg("Şifre başarıyla değiştirildi.");
      setPwForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
    } catch (ex) {
      setPwErr(ex.message || "Şifre değiştirilemedi");
    } finally {
      setSavingPw(false);
    }
  }

  if (loading) return <div className="card">Yükleniyor...</div>;
  if (err && !profile) return <div className="card error">{err}</div>;

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Profilim</h2>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginTop: 0 }}>Hesap Bilgileri</h3>
        <p style={{ color: "var(--muted)", fontSize: 13, marginTop: -6 }}>
          {profile.email} · {profile.role} · {profile.tenantName}
        </p>

        {err && <div className="error" style={{ marginBottom: 12 }}>{err}</div>}
        {profileMsg && (
          <div
            className="card"
            style={{ background: "#f0fdf4", border: "1px solid #16a34a", color: "#14532d", marginBottom: 12 }}
          >
            {profileMsg}
          </div>
        )}

        <form onSubmit={handleProfileSubmit}>
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="field">
              <label>Ad Soyad</label>
              <input
                required
                value={form.fullName}
                onChange={(e) => setForm({ ...form, fullName: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Departman</label>
              <input
                value={form.department}
                onChange={(e) => setForm({ ...form, department: e.target.value })}
              />
            </div>
          </div>
          <button className="btn" disabled={savingProfile}>
            {savingProfile ? "Kaydediliyor..." : "Profili Güncelle"}
          </button>
        </form>
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Şifre Değiştir</h3>

        {pwErr && <div className="error" style={{ marginBottom: 12 }}>{pwErr}</div>}
        {pwMsg && (
          <div
            className="card"
            style={{ background: "#f0fdf4", border: "1px solid #16a34a", color: "#14532d", marginBottom: 12 }}
          >
            {pwMsg}
          </div>
        )}

        <form onSubmit={handlePasswordSubmit}>
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="field">
              <label>Mevcut Şifre</label>
              <input
                required
                type="password"
                value={pwForm.currentPassword}
                onChange={(e) => setPwForm({ ...pwForm, currentPassword: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Yeni Şifre</label>
              <input
                required
                type="password"
                value={pwForm.newPassword}
                onChange={(e) => setPwForm({ ...pwForm, newPassword: e.target.value })}
              />
            </div>
            <div className="field">
              <label>Yeni Şifre (Tekrar)</label>
              <input
                required
                type="password"
                value={pwForm.confirmPassword}
                onChange={(e) => setPwForm({ ...pwForm, confirmPassword: e.target.value })}
              />
            </div>
          </div>
          <button className="btn" disabled={savingPw}>
            {savingPw ? "Değiştiriliyor..." : "Şifreyi Değiştir"}
          </button>
        </form>
      </div>
    </div>
  );
}
