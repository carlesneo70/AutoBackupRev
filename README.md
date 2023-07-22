# Dokumentasi Aplikasi AutoBackup.exe

<!-- Menggunakan ikon sebagai logo -->
<p align="center">
  <img src="icon.ico" alt="Logo" width="80" height="80">
</p>

## Deskripsi

Aplikasi AutoBackup.exe adalah utilitas untuk melakukan backup database MySQL. Aplikasi ini memungkinkan backup seluruh database dalam satu file, memisahkan backup berdasarkan tabel, atau membackup tabel-tabel tertentu.

**Informasi Tambahan**:
Aplikasi AutoBackup.exe sudah dipaket menjadi satu file portabel menggunakan bantuan Costura. Costura adalah alat yang menggabungkan semua dependensi atau file pendukung yang diperlukan oleh aplikasi menjadi satu file tunggal. Keuntungannya adalah pengguna tidak perlu menginstal dependensi tambahan atau memiliki file-file pendukung terpisah untuk menjalankan aplikasi.

## Cara Menggunakan

### Backup Full Database:

Untuk melakukan backup seluruh database dalam satu file, jalankan perintah berikut:

AutoBackup.exe <path_backup> server=<server_name>;port=<port_number>;database=<database_name>;user=<username>;password=<password>;

Contoh:
AutoBackup.exe D:\BACKUP\ server=localhost;port=3306;database=harmoni;user=root;password=123456;

### Backup Full Database Dibagi Berdasarkan Tabel:

Untuk memisahkan backup berdasarkan tabel dan menyimpan setiap tabel dalam file terpisah, gunakan parameter `alltables` seperti berikut:

AutoBackup.exe <path_backup> server=<server_name>;port=<port_number>;database=<database_name>;user=<username>;password=<password>; alltables

Contoh:
AutoBackup.exe D:\BACKUP\ server=localhost;port=3306;database=harmoni;user=root;password=123456; alltables

### Backup Database Berdasarkan Tabel yang Diperlukan:

Jika Anda hanya ingin membackup tabel-tabel tertentu, berikan nama-nama tabel sebagai parameter setelah `<path_backup>`, `<server_name>`, dan kredensial koneksi server:

AutoBackup.exe <path_backup> server=<server_name>;port=<port_number>;database=<database_name>;user=<username>;password=<password>; tabel <nama_tabel1> <nama_tabel2> <nama_tabel3> ...

Contoh:
AutoBackup.exe D:\BACKUP\ server=localhost;port=3306;database=wisata;user=root;password=123456; tabel tbl_akses tbl_users tbl_karyawan

## Catatan Penting

- Pastikan Anda memberikan izin akses yang sesuai ke direktori `<path_backup>`.
- Pastikan kredensial koneksi server yang benar agar aplikasi dapat terhubung ke server MySQL dan melakukan backup database dengan sukses.

## Disclaimer

Aplikasi AutoBackup.exe masih dalam tahap pengembangan dan mungkin memiliki beberapa kekurangan. Kami mengundang kontribusi dari para pengguna untuk membantu memperbaiki dan meningkatkan aplikasi ini. Jika Anda menemukan bug, memiliki saran, atau ingin berkontribusi dalam bentuk lainnya, silakan ajukan permintaan tarik (pull request) ke repositori di [link_to_repository](link_to_repository). Kami sangat menghargai kontribusi Anda untuk meningkatkan kualitas dan fungsionalitas aplikasi ini.

## Terima kasih telah menggunakan AutoBackup.exe