# WPF-NVM-Wizard Setup Guide

## Cài đặt công cụ WiX Toolset

### Bước 1: Cài đặt WiX Toolset
- Tải WiX Toolset tại đây: https://wixtoolset.org/releases/

### Bước 2: Cài đặt WiX Extension cho Visual Studio

## BUILDING WIX PROJECT

### Bước 1: Truy cập thư mục dự án
```
E:\Project\.NET\WpfNVMWizard\NVMWizardSetup
```

### Bước 2: Click chuột phải chọn Open in Terminal

### Bước 3: Chạy lệnh biên dịch tệp WiX
```
candle Product.wxs
```

### Bước 4: Chạy lệnh liên kết để tạo file cài đặt
```
light Product.wixobj -o NVMWizardSetup.msi
```
