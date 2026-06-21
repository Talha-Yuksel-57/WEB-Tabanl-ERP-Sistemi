import * as signalR from "@microsoft/signalr";
import { BASE_URL } from "./http";

// WebSocket/SignalR bağlantı nesnesini tek bir noktada (singleton gibi) tutan global değişken
let connection = null;

// Backend'deki ERP Hub'ına gerçek zamanlı bağlantıyı başlatan asenkron/dinamik fonksiyon
// Parametre olarak güncel kullanıcı token'ını ve gelen bildirimleri yakalayacak bir callback fonksiyonu alır
export function startSignalRConnection(token, onNotification) {
  // Eğer halihazırda aktif bir bağlantı varsa, yeni bir tane oluşturmak yerine mevcut olanı döndürür
  if (connection) return connection;

  // SignalR Hub bağlantı yapılandırması oluşturuluyor
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/erp`, {
      // Güvenlik için JWT token'ı URL'e (query string) eklemek yerine, 
      // SignalR'ın resmi ve güvenli yöntemi olan accessTokenFactory ile HTTP üst bilgisine (header) gömer.
      accessTokenFactory: () => token,
    })
    // Ağ kopmaları durumunda tarayıcının backend'e otomatik olarak yeniden bağlanmasını (reconnect) sağlar
    .withAutomaticReconnect()
    // Konsol kalabalığını önlemek için sadece 'Warning' (Uyarı) ve üzerindeki kritik logları konsola basar
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  // Backend (Hub) tarafından tetiklenen "ReceiveNotification" isimli event'i (olayı) dinler.
  // Sunucudan her yeni bildirim geldiğinde, parametre olarak gelen 'onNotification' fonksiyonunu çalıştırır.
  connection.on("ReceiveNotification", (notification) => {
    onNotification?.(notification);
  });

  // Yapılandırılan SignalR bağlantısını asenkron olarak başlatır
  connection.start().catch((err) => {
    console.warn("SignalR bağlantısı kurulamadı:", err.message);
  });

  return connection;
}

// Kullanıcı çıkış yaptığında veya bileşen (component) kapandığında 
// açık olan canlı bağlantıyı kapatıp belleği temizleyen fonksiyon
export function stopSignalRConnection() {
  if (connection) {
    connection.stop(); // Aktif WebSocket akışını sonlandırır
    connection = null; // Bağlantı referansını sıfırlar
  }
}