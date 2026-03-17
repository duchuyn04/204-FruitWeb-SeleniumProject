# Hướng dẫn Branch & Push lên GitHub

## Phân công branch

| Branch | Người phụ trách |
|---|---|
| `main` | Không ai làm trực tiếp |
| `feature/person1` | Thành viên 1 |
| `feature/person2` | Thành viên 2 |
| `feature/person3` | Thành viên 3 |

---

## Lần đầu tiên (clone về máy)

```bash
# 1. Clone project về máy
git clone https://github.com/duchuy19012004/204-FruitWeb-SeleniumProject.git

# 2. Vào thư mục project
cd 204-FruitWeb-SeleniumProject/SeleniumProject

# 3. Chuyển sang branch của mình (thay person2/person3 tùy người)
git checkout feature/person2
```

---

## Quy trình làm việc hàng ngày

```bash
# Bước 1 — Mỗi sáng: lấy code mới nhất từ main về branch của mình
git fetch origin
git merge origin/main

# Bước 2 — Làm việc bình thường (viết code, test...)

# Bước 3 — Lưu thay đổi
git add .
git commit -m "test(auth): thêm test cases cho login"

# Bước 4 — Push lên GitHub
git push origin feature/person1
```

---

## Khi hoàn thành 1 module → Merge vào main

```
1. Lên GitHub
2. Vào tab "Pull requests" → click "New pull request"
3. base: main ← compare: feature/person1
4. Điền tiêu đề và mô tả
5. Chờ người khác review → Merge
```

---

## Kiểm tra đang ở branch nào

```bash
git branch
# Branch đang dùng sẽ có dấu * ở đầu
# * feature/person1
#   feature/person2
#   main
```

---

## Quy tắc quan trọng

| ✅ Nên làm | ❌ Không làm |
|---|---|
| Làm việc trên branch của mình | Commit thẳng vào `main` |
| Pull main mỗi sáng | Sửa file trong `Utilities/` mà không báo nhóm |
| Commit thường xuyên, message rõ ràng | Để quá nhiều thay đổi mới commit 1 lần |
| Tạo Pull Request khi xong module | Push thẳng vào `main` |
