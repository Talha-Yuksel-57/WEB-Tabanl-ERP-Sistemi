import { useEffect, useState } from "react";
import { httpGet, httpPost, httpPatch } from "../api/http";

// Uygulamanın çoklu kiracı (multi-tenant) destekli ürün yönetim sayfa bileşeni
export default function ProductsPage({ token, tenantId }) {
  // --- STATE (DURUM) TANIMLAMALARI ---
  const [items, setItems] = useState([]);       // Sunucudan çekilen ürün listesini tutan dizi
  const [err, setErr] = useState(null);         // Hata mesajlarını ekranda göstermek için kullanılan state
  const [loading, setLoading] = useState(false); // Veri yüklenirken arayüzde kilitlenme/bekleme durumunu yöneten bayrak

  // Yeni eklenecek ürünün form alanlarını tutan durum değişkenleri (Varsayılan değerleriyle başlatılmıştır)
  const [name, setName] = useState("Yeni Ürün");
  const [price, setPrice] = useState(100);
  const [minStockLevel, setMinStockLevel] = useState(5);
  const [category, setCategory] = useState("Genel");

  // --- API ASENKRON İŞLEMLERİ ---

  // Aktif kiracıya (Tenant) ait ürünleri backend'den çeken fonksiyon
  async function load(){
    setErr(null);
    setLoading(true);
    try{
      // Çoklu kiracılık mimarisinde her isteğin hangi firmaya ait olduğunu belirtmek için token ve tenantId parametre olarak geçilir
      const data = await httpGet("/api/Products", { token, tenantId });
      setItems(data || []);
    }catch(ex){
      setErr(ex.message);
    }finally{
      setLoading(false);
    }
  }

  // Formdaki verileri kullanarak yeni bir ürün oluşturan fonksiyon
  async function create(){
    setErr(null);
    try{
      // Veri güvenliği ve çoklu kiracılık doğrulaması: tenantId yoksa isteğin atılması engellenir
      if(!tenantId) throw new Error("Tenant seçmeden ürün göremez/ekleyemezsin. Sağ üstten Tenant seç.");
      
      const body = { name, price, stockCount: 0, minStockLevel, category }; // Yeni ürün şablonu (Stok ilk başta 0 set edilir)
      await httpPost("/api/Products", body, { token, tenantId });
      await load(); // Ekleme işleminden sonra güncel listeyi çekmek için tabloyu yeniler
    }catch(ex){
      setErr(ex.message);
    }
  }

  // Ürünün stok miktarını hızlıca (+1 / -1) güncelleyen kısmi güncelleme (PATCH) fonksiyonu
  async function updateStock(id, newStock){
    setErr(null);
    try{
      if(!tenantId) throw new Error("Tenant seçmeden stok güncelleyemezsin.");
      
      // Backend'deki ilgili ürünün 'update-stock' uç noktasına (endpoint) yeni stok miktarını sayı tipinde gönderir
      await httpPatch(`/api/Products/${id}/update-stock`, Number(newStock), { token, tenantId });
      await load(); // Stok değişiminden sonra güncel stok sayılarını görmek için tabloyu yeniler
    }catch(ex){
      setErr(ex.message);
    }
  }

  // Sayfa ilk yüklendiğinde ya da kullanıcının firması (tenantId) veya oturumu (token) değiştiğinde tetiklenen Effect.
  // Bu sayede veri izolasyonu sağlanır; kullanıcı sağ üstten farklı firma seçtiğinde tablo anında o firmaya göre güncellenir.
  useEffect(()=>{ load(); }, [tenantId, token]);

  // --- ARAYÜZ (JSX) RENDER ALANI ---
  return (
    <div className="card">
      <h2 style={{marginTop:0}}>Products</h2>
      {/* Çoklu kiracılık mimarisinde HTTP Header (X-Tenant-Id) kullanım gereksinimini belirten teknik not */}
      <div className="small">Multi-tenant filtreleme için <b>X-Tenant-Id</b> zorunlu.</div>
      
      {/* Varsa Hata Mesajı Paneli */}
      {err && <div className="error" style={{marginTop: 10}}>{err}</div>}

      {/* Ürün Ekleme Form Alanı */}
      <div className="row" style={{marginTop: 10}}>
        <div className="field">
          <label>Name</label>
          <input value={name} onChange={(e)=>setName(e.target.value)} />
        </div>
        <div className="field">
          <label>Price</label>
          <input type="number" value={price} onChange={(e)=>setPrice(Number(e.target.value))} />
        </div>
        <div className="field">
          <label>MinStockLevel</label>
          <input type="number" value={minStockLevel} onChange={(e)=>setMinStockLevel(Number(e.target.value))} />
        </div>
        <div className="field">
          <label>Category</label>
          <input value={category} onChange={(e)=>setCategory(e.target.value)} />
        </div>
        <button className="btn" onClick={create}>Ürün Ekle</button>
        <button className="btn secondary" onClick={load} disabled={loading}>{loading ? "Yükleniyor..." : "Yenile"}</button>
      </div>

      {/* Dinamik Ürün Listesi Tablosu */}
      <table className="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Ad</th>
            <th>Kategori</th>
            <th>Fiyat</th>
            <th>Stok</th>
            <th>MinStok</th>
            <th>Hızlı</th> {/* Hızlı stok artırma/azaltma buton kolon başlığı */}
          </tr>
        </thead>
        <tbody>
          {items?.map(p => (
            <tr key={p.id}>
              <td>{p.id}</td>
              <td>{p.name}</td>
              <td>{p.category}</td>
              <td>{p.price}</td>
              <td>{p.stockCount}</td>
              <td>{p.minStockLevel}</td>
              <td className="row">
                {/* Stok azaltma butonu: Math.max(0, ...) kontrolü ile stoğun eksiye düşmesi arayüz düzeyinde engellenir */}
                <button className="btn secondary" onClick={()=>updateStock(p.id, Math.max(0, (p.stockCount ?? 0) - 1))}>-1</button>
                {/* Stok artırma butonu */}
                <button className="btn secondary" onClick={()=>updateStock(p.id, (p.stockCount ?? 0) + 1)}>+1</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Sayfa Alt Bilgisi: Sayfanın backend üzerinde hangi RESTful endpoint'leri tükettiğini (consume ettiğini) gösteren teknik etiketler */}
      <div className="small">
        Backend: <code>GET /api/Products</code>, <code>POST /api/Products</code>, <code>PATCH /api/Products/{"{id}"}/update-stock</code>
      </div>
    </div>
  );
}