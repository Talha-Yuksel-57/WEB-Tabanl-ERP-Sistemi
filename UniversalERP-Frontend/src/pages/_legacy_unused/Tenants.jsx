import { useEffect, useState } from "react";
import { httpGet, httpPost } from "../api/http";

// Uygulama genelindeki tüm kiracıları (firmaları) yöneten sistem yönetimi bileşeni
// Not: Bu sayfa genellikle sadece SuperAdmin rolünün erişimine açık olacak şekilde kurgulanır.
export default function TenantsPage({ token }) {
  // --- STATE (DURUM) TANIMLAMALARI ---
  const [items, setItems] = useState([]);       // Sistemde kayıtlı olan tüm firmaların listesi
  const [err, setErr] = useState(null);         // Olası yetki veya sunucu hatalarını ekranda göstermek için state
  const [loading, setLoading] = useState(false); // Yenileme ve yükleme esnasındaki asenkron durum bayrağı

  // Yeni oluşturulacak örnek firmanın form alanları ve varsayılan başlangıç değerleri
  const [name, setName] = useState("Demo Firma");
  const [industry, setIndustry] = useState("market");
  const [taxNumber, setTaxNumber] = useState("1111111111");
  const [address, setAddress] = useState("Bursa");
  const [planType, setPlanType] = useState("basic");

  // --- API ASENKRON İŞLEMLERİ ---

  // Sistemdeki tüm firmaların listesini backend API'den güvenli bir şekilde çeken fonksiyon
  async function load() {
    setErr(null);
    setLoading(true);
    try {
      // Backend tarafında bu endpoint korumalı (Authorize) olduğu için 
      // kimlik doğrulama amacıyla nesne içinde 'token' bilgisi gönderilir.
      const data = await httpGet("/api/Tenants", { token });
      setItems(data || []);
    } catch (ex) {
      setErr(ex.message);
    } finally {
      setLoading(false);
    }
  }

  // Formdaki bilgileri kullanarak sisteme yeni bir kiracı (firma) ekleyen fonksiyon
  async function create() {
    setErr(null);
    try {
      const body = { name, industry, taxNumber, address, planType };
      // Yeni firma kaydederken de SuperAdmin yetkisinin doğrulanması için 'token' parametresi iletilir
      await httpPost("/api/Tenants", body, { token });
      await load(); // Ekleme işlemi başarılı olduktan sonra yeni firmayı tabloda görmek için listeyi yeniler
    } catch (ex) {
      setErr(ex.message);
    }
  }

  // Bileşen ekrana çizildiğinde veya kullanıcının oturum token'ı değiştiğinde tetiklenen Effect.
  // Güvenlik kontrolü: Kullanıcı sisteme login olmuşsa (yani token mevcutsa) veri çekme fonksiyonunu çalıştırır.
  useEffect(() => { 
    if(token) load(); 
  }, [token]);

  // --- ARAYÜZ (JSX) RENDER ALANI ---
  return (
    <div className="card">
      <h2 style={{ marginTop: 0 }}>Tenants</h2>
      
      {/* Varsa Hata Paneli */}
      {err && <div className="error">{err}</div>}

      {/* Yeni Firma Oluşturma Form Satırı */}
      <div className="row">
        <div className="field">
          <label>Name</label>
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="field">
          <label>Industry</label>
          {/* Sektör bazlı dropdown/select seçimi */}
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
          {/* Firmanın üyelik paketini belirleyen select yapısı */}
          <select value={planType} onChange={(e) => setPlanType(e.target.value)}>
            <option value="basic">basic</option>
            <option value="pro">pro</option>
          </select>
        </div>
        <div className="field" style={{ minWidth: 280 }}>
          <label>Address</label>
          <input value={address} onChange={(e) => setAddress(e.target.value)} />
        </div>

        {/* Aksiyon Butonları */}
        <button className="btn" onClick={create}>Yeni Tenant Oluştur</button>
        <button className="btn secondary" onClick={load} disabled={loading}>{loading ? "Yükleniyor..." : "Yenile"}</button>
      </div>

      {/* Tüm Kiracıların (Firmaların) Listelendiği HTML Tablosu */}
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

      {/* Sayfa Alt Bilgisi: Sayfanın tükettiği genel (Global-Level) backend uç noktaları */}
      <div className="small">
        Backend: <code>GET /api/Tenants</code>, <code>POST /api/Tenants</code>
      </div>
    </div>
  );
}