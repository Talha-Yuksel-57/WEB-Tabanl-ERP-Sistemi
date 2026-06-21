import { useEffect, useState } from "react";
import { httpGet, httpPost } from "../api/http";

// Fonksiyonun içine { token } ekledik ki yetki alabilsin
export default function TenantsPage({ token }) {
  const [items, setItems] = useState([]);
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(false);

  const [name, setName] = useState("Demo Firma");
  const [industry, setIndustry] = useState("market");
  const [taxNumber, setTaxNumber] = useState("1111111111");
  const [address, setAddress] = useState("Bursa");
  const [planType, setPlanType] = useState("basic");

  // Mevcut load fonksiyonu token ile çalışacak şekilde güncellendi
  async function load() {
    setErr(null);
    setLoading(true);
    try {
      // Backend yetki istediği için token'ı buraya ekledik
      const data = await httpGet("/api/Tenants", { token });
      setItems(data || []);
    } catch (ex) {
      setErr(ex.message);
    } finally {
      setLoading(false);
    }
  }

  // Yeni Tenant oluştururken de yetki (token) gerekli
  async function create() {
    setErr(null);
    try {
      const body = { name, industry, taxNumber, address, planType };
      await httpPost("/api/Tenants", body, { token });
      await load();
    } catch (ex) {
      setErr(ex.message);
    }
  }

  // Sayfa açıldığında verileri çek
  useEffect(() => { 
    if(token) load(); 
  }, [token]);

  return (
    <div className="card">
      <h2 style={{ marginTop: 0 }}>Tenants</h2>
      {err && <div className="error">{err}</div>}

      <div className="row">
        <div className="field">
          <label>Name</label>
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="field">
          <label>Industry</label>
          <select value={industry} onChange={(e) => setIndustry(e.target.value)}>
            <option value="market">market</option>
            <option value="repair">repair</option>
            <option value="software">software</option>
          </select>
        </div>
        <div className="field">
          <label>TaxNumber</label>
          <input value={taxNumber} onChange={(e) => setTaxNumber(e.target.value)} />
        </div>
        <div className="field">
          <label>PlanType</label>
          <select value={planType} onChange={(e) => setPlanType(e.target.value)}>
            <option value="basic">basic</option>
            <option value="pro">pro</option>
          </select>
        </div>
        <div className="field" style={{ minWidth: 280 }}>
          <label>Address</label>
          <input value={address} onChange={(e) => setAddress(e.target.value)} />
        </div>

        <button className="btn" onClick={create}>Yeni Tenant Oluştur</button>
        <button className="btn secondary" onClick={load} disabled={loading}>{loading ? "Yükleniyor..." : "Yenile"}</button>
      </div>

      <table className="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Industry</th>
            <th>Plan</th>
            <th>Tax</th>
            <th>Address</th>
          </tr>
        </thead>
        <tbody>
          {items?.map(t => (
            <tr key={t.id}>
              <td>{t.id}</td>
              <td>{t.name}</td>
              <td>{t.industry}</td>
              <td>{t.planType}</td>
              <td>{t.taxNumber}</td>
              <td>{t.address}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="small">
        Backend: <code>GET /api/Tenants</code>, <code>POST /api/Tenants</code>
      </div>
    </div>
  );
}