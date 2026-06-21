import { useEffect, useState } from "react";
import { getCustomers, createCustomer, updateCustomer, deleteCustomer } from "../api/customers";
import { hasRole, CAN_DELETE_CUSTOMERS } from "../utils/permissions";

// Yeni müşteri eklerken formu sıfırlamak için kullanılan boş form şablonu (initial state)
const emptyForm = { fullName: "", email: "", phone: "", address: "" };

export default function CustomersPage({ role }) {
  // --- STATE (DURUM) TANIMLAMALARI ---
  const [customers, setCustomers] = useState([]); // Backend'den çekilen müşterilerin tutulduğu dizi
  const [loading, setLoading] = useState(true);   // Sayfa yüklenme/veri çekme durumunu kontrol eden bayrak
  const [err, setErr] = useState(null);           // Oluşan hata mesajlarını kullanıcıya göstermek için tutulan state
  const [showForm, setShowForm] = useState(false); // Ekleme/Düzenleme form kartının görünürlüğünü kontrol eder
  const [editingId, setEditingId] = useState(null);// Düzenlenen müşterinin ID'sini tutar (null ise işlem "Yeni Kayıt"tır)
  const [form, setForm] = useState(emptyForm);     // Formdaki input değerlerini anlık olarak tutan nesne
  const [saving, setSaving] = useState(false);     // Kaydet butonuna basıldığında mükerrer isteği önleyen yüklenme durumu

  // Giriş yapan kullanıcının rolüne göre müşteri silme yetkisi olup olmadığını kontrol eder
  const canDelete = hasRole(role, CAN_DELETE_CUSTOMERS);

  // --- API İŞLEMLERİ VE FONKSİYONLAR ---

  // Backend API'den müşterileri çeken ve state'e aktaran asenkron fonksiyon
  async function loadCustomers() {
    setLoading(true); 
    setErr(null);
    try { 
      const data = await getCustomers(); 
      setCustomers(data || []); 
    }
    catch (ex) { 
      setErr(ex.message || "Müşteriler yüklenemedi"); 
    }
    finally { 
      setLoading(false); 
    }
  }

  // Bileşen ilk kez ekrana çizildiğinde (componentDidMount) müşteri listesini otomatik yükler
  useEffect(() => { loadCustomers(); }, []);

  // "Yeni Müşteri" butonuna basıldığında formu temizleyerek ekrana getiren fonksiyon
  function openCreateForm() { 
    setEditingId(null); 
    setForm(emptyForm); 
    setShowForm(true); 
  }

  // Satırdaki "Düzenle" butonuna basıldığında ilgili müşterinin bilgilerini forma dolduran fonksiyon
  function openEditForm(c) {
    setEditingId(c.id);
    setForm({ 
      fullName: c.fullName, 
      email: c.email || "", 
      phone: c.phone || "", 
      address: c.address || "" 
    });
    setShowForm(true);
  }

  // Form gönderildiğinde (Ekleme veya Güncelleme) tetiklenen ana fonksiyon
  async function handleSubmit(e) {
    e.preventDefault(); // Sayfanın yeniden yüklenmesini (default form submit) engeller
    setSaving(true); 
    setErr(null);
    try {
      // Backend'e gönderilecek veri transfer nesnesi (DTO) hazırlanıyor. Boş alanlar null olarak set edilir.
      const dto = { 
        fullName: form.fullName, 
        email: form.email || null, 
        phone: form.phone || null, 
        address: form.address || null 
      };
      
      // Eğer editingId varsa PUT (güncelleme), yoksa POST (yeni kayıt) isteği gönderilir
      if (editingId) await updateCustomer(editingId, dto);
      else await createCustomer(dto);
      
      setShowForm(false); // İşlem başarılıysa formu kapat
      await loadCustomers(); // Tablodaki listeyi güncel değerlerle yenile
    } catch (ex) { 
      setErr(ex.message || "Kayıt başarısız"); 
    }
    finally { 
      setSaving(false); 
    }
  }

  // Bir müşteriyi sistemden silen asenkron fonksiyon
  async function handleDelete(id) {
    if (!confirm("Bu müşteriyi silmek istediğinize emin misiniz?")) return; // Kullanıcıdan onay alır
    setErr(null);
    try { 
      await deleteCustomer(id); 
      await loadCustomers(); // Silme işleminden sonra listeyi tazeleyip arayüzü günceller
    }
    catch (ex) { 
      setErr(ex.message || "Silme başarısız"); 
    }
  }

  // --- ARAYÜZ (JSX) RENDER ALANI ---
  return (
    <div>
      {/* Sayfa Başlığı ve Yeni Müşteri Ekleme Butonu */}
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Müşteriler</h2>
        <button className="btn" onClick={openCreateForm}>+ Yeni Müşteri</button>
      </div>

      {/* Varsa Hata Mesajının Gösterileceği Dinamik Alan */}
      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {/* Dinamik Ekleme / Düzenleme Form Kartı (Yalnızca showForm true ise render olur) */}
      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>{editingId ? "Müşteri Düzenle" : "Yeni Müşteri"}</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Ad Soyad</label>
                {/* İki yönlü veri bağlama (Two-way data binding) ile input değiştikçe form state'i güncellenir */}
                <input required value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
              </div>
              <div className="field">
                <label>E-posta</label>
                <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </div>
              <div className="field">
                <label>Telefon</label>
                <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </div>
              <div className="field">
                <label>Adres</label>
                <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
              </div>
            </div>
            {/* Form Aksiyon Butonları */}
            <div className="row">
              {/* Kaydetme esnasında buton deaktif edilerek çift tıklama ile mükerrer kayıt atılması önlenir */}
              <button className="btn" disabled={saving}>{saving ? "Kaydediliyor..." : "Kaydet"}</button>
              <button type="button" className="btn secondary" onClick={() => setShowForm(false)}>Vazgeç</button>
            </div>
          </form>
        </div>
      )}

      {/* Müşteri Listesinin Tablo Halinde Gösterileceği Kart Yapısı */}
      <div className="card">
        {loading && <div>Yükleniyor...</div>}
        
        {/* Veri yüklenmesi bittiğinde listede hiç eleman yoksa gösterilecek alan */}
        {!loading && customers.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz müşteri eklenmemiş.</div>
        )}
        
        {/* Veriler başarıyla yüklendiğinde oluşacak dinamik HTML Tablosu */}
        {!loading && customers.length > 0 && (
          <table className="table">
            <thead>
              <tr><th>Ad Soyad</th><th>E-posta</th><th>Telefon</th><th>Adres</th><th></th></tr>
            </thead>
            <tbody>
              {customers.map((c) => (
                <tr key={c.id}>
                  <td>{c.fullName}</td>
                  <td>{c.email || "-"}</td>
                  <td>{c.phone || "-"}</td>
                  <td>{c.address || "-"}</td>
                  <td>
                    <div className="row">
                      <button className="btn secondary" onClick={() => openEditForm(c)}>Düzenle</button>
                      {/* Silme butonu sadece 'canDelete' yetkisine sahip olan rollere (Örn: Admin) özel olarak render edilir */}
                      {canDelete && (
                        <button className="btn secondary" onClick={() => handleDelete(c.id)}>Sil</button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}