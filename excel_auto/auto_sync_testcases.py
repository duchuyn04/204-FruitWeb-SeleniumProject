import os
import time
import win32com.client

# ==========================================
# 📖 HƯỚNG DẪN SỬ DỤNG:
# ==========================================
# Script này giúp tự động hóa quá trình ánh xạ ID giữa Requirement và Test Case.
# Kịch bản sử dụng khi làm cho 1 chức năng MỚI (Ví dụ: Account Management):
# 
# Bước 1: Sang sheet Test Requirement, bôi đen tìm điểm đầu & cuối của chức năng.
# Bước 2: Sửa 2 biến `REQ_START_ROW` và `REQ_END_ROW` ở dưới.
# Bước 3: Sửa `TC_SHEET_NAME` thành tên Sheet chứa Test Case.
# Bước 4: Mở Terminal chạy file này (python excel_auto/auto_sync_testcases.py).
# ==========================================
# ⚙️ CẤU HÌNH BAN ĐẦU (ĐIỀN TRƯỚC KHI CHẠY)
# ==========================================

EXCEL_FILE_PATH = r'C:\Users\juven\OneDrive\Documents\204.xlsx'


REQ_SHEET_NAME  = 'Test Requirement'
TC_SHEET_NAME   = 'TC_OrderManagement'  # Đổi thành TC_Khac nếu làm module khác

# Xác định module này nằm từ hàng nào đến hàng nào bên sheet Test Requirement
REQ_START_ROW = 177
REQ_END_ROW   = 208

# Dòng khởi đầu chứa dữ liệu bên sheet Test Case (Sau Header)
TC_START_ROW = 12

def run_sync():
    print(f"[*] Đang kết nối tới Application Excel (win32com)...")
    
    # ---------------------------------------------------------
    # Dùng COM Interop để điều khiển chính Excel gốc của hệ thống.
    # Nhờ đó, OneDrive AutoSave giữ nguyên nguyên trạng & không bị conflict!
    # ---------------------------------------------------------
    try:
        excel = win32com.client.Dispatch("Excel.Application")
    except Exception as e:
        print("[!] Không gọi được Excel.Application, đảm bảo máy bạn cài Word/Excel đầy đủ.")
        return

    excel.Visible = True # Cố tình bật lên để quá trình AutoSave của OneDrive nhận diện

    wb = None
    try:
        # Nếu file đã được mở sẵn trong Excel thì tận dụng lại quá trình đó:
        for workbook in excel.Workbooks:
            if str(workbook.FullName).lower() == EXCEL_FILE_PATH.lower():
                wb = workbook
                print("[*] Đã tìm thấy file đang mở sẵn.")
                break
        
        # Nếu chưa mở thì mở mới
        if not wb:
            print(f"[*] Đang mở file: {EXCEL_FILE_PATH}...")
            wb = excel.Workbooks.Open(EXCEL_FILE_PATH)

        ws_req = wb.Sheets(REQ_SHEET_NAME)
        ws_tc = wb.Sheets(TC_SHEET_NAME)

        # ---------------------------------------------------------
        # BƯỚC 1: XỬ LÝ BÊN SHEET "TEST REQUIREMENT" (Cột I & J)
        # ---------------------------------------------------------
        print(f"[*] Đang gắn công thức CountIF & Hyperlink vào '{REQ_SHEET_NAME}'...")
        req_mapping_dict = {}

        for r in range(REQ_START_ROW, REQ_END_ROW + 1):
            req_id_val = ws_req.Cells(r, 2).Value
            if req_id_val:
                req_mapping_dict[str(req_id_val).strip()] = r
                
            f_count = f'=IF(COUNTIF(\'{TC_SHEET_NAME}\'!B:B,B{r})=0,"",COUNTIF(\'{TC_SHEET_NAME}\'!B:B,B{r}))'
            ws_req.Cells(r, 9).Formula = f_count
            
            f_link = f'=IF(I{r}="","",HYPERLINK("#\'{TC_SHEET_NAME}\'!A"&MATCH(B{r},\'{TC_SHEET_NAME}\'!B:B,0),"Go to TC"))'
            ws_req.Cells(r, 10).Formula = f_link

        # ---------------------------------------------------------
        # BƯỚC 2: XỬ LÝ BÊN SHEET "TEST CASE" (Ánh xạ ID & Generate TC_ID)
        # ---------------------------------------------------------
        print(f"[*] Đang rà quét và tự động sinh mã liên kết bên '{TC_SHEET_NAME}'...")
        
        # Tìm max_row của sheet TC dựa trên cột A và B
        max_row_a = ws_tc.Cells(ws_tc.Rows.Count, 1).End(-4162).Row # xlUp = -4162
        max_row_b = ws_tc.Cells(ws_tc.Rows.Count, 2).End(-4162).Row
        max_row = max(max_row_a, max_row_b)

        if max_row < TC_START_ROW: 
            max_row = TC_START_ROW

        blocks = []
        current_start = TC_START_ROW
        
        for r in range(TC_START_ROW + 1, max_row + 1):
            val_a = ws_tc.Cells(r, 1).Value
            # Tách block test case theo số thứ tự ở Cột A
            if val_a is not None and str(val_a).strip() != '':
                blocks.append((current_start, r - 1))
                current_start = r
                
        blocks.append((current_start, max_row)) # Block cuối

        missing_mappings = 0
        covered_reqs = set()
        
        for (r_start, r_end) in blocks:
            # Lấy chuỗi ID, đọc từ thư viện COM
            raw_req_id = ws_tc.Cells(r_start, 2).Value
            
            # Hàm Value của Formula đôi khi trả về int hoặc float nếu là số hóa (e.g. F12 -> error nhưng 12.1 thàh số). Cần format lại:
            req_id_str = str(raw_req_id).strip() if raw_req_id else ""
            
            if req_id_str in req_mapping_dict:
                target_row = req_mapping_dict[req_id_str]
                covered_reqs.add(req_id_str)
                
                # 1. Cột N: Đánh dấu Lưu vết Tham chiếu
                ws_tc.Cells(r_start, 14).Value = target_row
                
                # 2. Cột B: Kéo Link động đến đúng dòng bên kia thay vì giá trị thô cứng
                ws_tc.Cells(r_start, 2).Formula = f"='{REQ_SHEET_NAME}'!B{target_row}"
                
                # 3. Cột C: Tự động đếm và chèn hậu tố số thự tự Test Case
                ws_tc.Cells(r_start, 3).Formula = f'="TC_" & B{r_start} & "_" & TEXT(COUNTIF(B${TC_START_ROW}:B{r_start}, B{r_start}), "00")'
            else:
                print(f"  [Cảnh báo] Lỗi mapping tại dòng {r_start}: Sheet TC ghi mã là '{req_id_str}' nhưng không tìm thấy bên Test Requirement!")
                missing_mappings += 1

        uncovered_reqs = set(req_mapping_dict.keys()) - covered_reqs
        if uncovered_reqs:
            print(f"\n[!] THIẾU SÓT: Có {len(uncovered_reqs)} Requirement đang trống (Chưa có Test Case nào):")
            for req in sorted(uncovered_reqs):
                print(f"  -> {req} (tại dòng {req_mapping_dict[req]} bên Requirement)")
        else:
            print("\n[v] Tuyệt vời! Tất cả các Requirement đều đã có Test Case bao phủ.")

        # ---------------------------------------------------------
        # KẾT THÚC VÀ LƯU
        # ---------------------------------------------------------
        print("\n[*] Đang lưu file thông qua Excel AutoSave...")
        wb.Save()

        print(f"[v] Đã lưu thông qua chính App Excel! Giao thức OneDrive hoàn toàn không bị conflict.")
        print(f"[v] Tổng cộng có {len(blocks)} Test Case (kịch bản) được xử lý.")
        print(f"[v] Lỗi mapping sai ID: {missing_mappings} kịch bản.")

    except Exception as e:
        print(f"[!] Đã xảy ra lỗi: {e}")

if __name__ == "__main__":
    run_sync()

