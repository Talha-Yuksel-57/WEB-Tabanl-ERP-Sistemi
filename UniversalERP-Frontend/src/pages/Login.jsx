import { useState } from "react";
import { login } from "../api/auth";

export default function LoginPage({ onLogin, onShowRegister }) {
  const [email, setEmail] = useState("master@erp.com");
  const [password, setPassword] = useState("Master123!");
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setErr(null);
    setLoading(true);
    try {
      // Backend: { accessToken, refreshToken, expiresAt, fullName, email, role, tenantId }
      const data = await login(email, password);
      if (!data?.accessToken) {
        throw new Error("Token alınamadı. Sunucu cevabı beklenenden farklı.");
      }
      onLogin(data);
    } catch (ex) {
      setErr(ex.message || "Giriş başarısız");
      console.error(ex);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: 420, margin: "60px auto" }}>
        <h2 style={{ marginTop: 0 }}>UniversalERP</h2>
        <p style={{ color: "var(--muted)", marginTop: -8, marginBottom: 18 }}>
          Hesabınla giriş yap
        </p>

        {err && (
          <div className="error" style={{ marginBottom: 12 }}>
            {err}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="field" style={{ marginBottom: 12 }}>
            <label>E-posta</label>
            <input
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              style={{ width: "100%" }}
            />
          </div>
          <div className="field" style={{ marginBottom: 18 }}>
            <label>Şifre</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              style={{ width: "100%" }}
            />
          </div>
          <button className="btn" disabled={loading} style={{ width: "100%" }}>
            {loading ? "Giriş yapılıyor..." : "Giriş Yap"}
          </button>
        </form>

        <p style={{ textAlign: "center", marginTop: 16, color: "var(--muted)", fontSize: 13 }}>
          Hesabınız yok mu?{" "}
          <a href="#" onClick={(e) => { e.preventDefault(); onShowRegister?.(); }}>
            Kayıt Ol
          </a>
        </p>
      </div>
    </div>
  );
}
