import win32com.client

# ==========================================
# CẤU HÌNH
# ==========================================

EXCEL_FILE_PATH = r'C:\Users\juven\OneDrive\Documents\204.xlsx'

# Danh sách sheet cần gắn công thức — thêm sheet mới vào đây khi mở module mới
SHEETS_TO_UPDATE = [
    'TC_CheckoutOrder',
    # 'TC_Auth',  # Thêm các sheet khác tại đây
]

# Cột chứa kết quả Pass/Fail (L = cột 12)
RESULT_COL = 13

# Dòng bắt đầu chứa data (sau header)
DATA_START_ROW = 13

# Dòng chứa công thức tổng (Pass, Fail, Untested, N/A, Total)
SUMMARY_ROW = 7

# ==========================================
# LOGIC CHÍNH
# ==========================================

def get_last_data_row(ws, result_col):
    """Tìm dòng cuối cùng có dữ liệu — dò theo cột A (No.) vì cột L có thể còn trống."""
    last_row = ws.UsedRange.Rows.Count
    # Dò ngược từ cuối lên cột A để tìm dòng có số thứ tự
    for r in range(last_row, DATA_START_ROW - 1, -1):
        val = ws.Cells(r, 1).Value  # Cột A = No.
        if val is not None and str(val).strip() != '':
            return r
    return DATA_START_ROW


def col_letter(col_num):
    """Chuyển số cột sang chữ (1=A, 12=L, 13=M...)."""
    result = ''
    while col_num > 0:
        col_num, remainder = divmod(col_num - 1, 26)
        result = chr(65 + remainder) + result
    return result


def write_summary_formulas(ws):
    res_col_letter = col_letter(RESULT_COL)   # VD: 12→L, 13→M
    tc_col_letter  = col_letter(3)             # Cột C = Test Case ID
    end_row = get_last_data_row(ws, RESULT_COL)
    data_range    = f'{res_col_letter}{DATA_START_ROW}:{res_col_letter}{end_row}'
    tc_id_range   = f'{tc_col_letter}{DATA_START_ROW}:{tc_col_letter}99993'

    formulas = {
        1: f'=COUNTIF({data_range},"Passed")',        # Pass
        2: f'=COUNTIF({data_range},"Failed")',         # Fail
        3: f'=COUNTIF({data_range},"Untested")',       # Untested
        4: f'=COUNTIF({data_range},"N/A")',            # N/A
        5: f'=COUNTIF({tc_id_range},"TC_*")',          # Number of Test Cases — đếm ô bắt đầu TC_
    }

    sheet_name = ws.Name
    for col, formula in formulas.items():
        ws.Cells(SUMMARY_ROW, col).Formula = formula

    print(f'  [v] Sheet "{sheet_name}": gắn công thức vào Row {SUMMARY_ROW}, phạm vi {data_range} (đến row {end_row})')


def run():
    print('[*] Đang kết nối tới Excel (win32com)...')

    # Ưu tiên dùng Excel đang chạy sẵn (giữ AutoSave OneDrive), nếu không thì mở mới
    try:
        excel = win32com.client.GetActiveObject("Excel.Application")
    except Exception:
        excel = win32com.client.Dispatch("Excel.Application")
        excel.Visible = True

    # Tìm workbook 204.xlsx đang mở, nếu chưa có thì mở file
    wb = None
    try:
        for workbook in excel.Workbooks:
            if '204.xlsx' in workbook.Name:
                wb = workbook
                break
    except Exception:
        pass

    if not wb:
        print(f'[*] Đang mở file: {EXCEL_FILE_PATH}...')
        wb = excel.Workbooks.Open(EXCEL_FILE_PATH)

    print(f'[*] Đang gắn công thức COUNTIF vào dòng {SUMMARY_ROW}...')
    for sheet_name in SHEETS_TO_UPDATE:
        try:
            ws = wb.Sheets(sheet_name)
            write_summary_formulas(ws)
        except Exception as e:
            print(f'  [!] Bỏ qua sheet "{sheet_name}": {e}')

    # Lưu qua Excel để OneDrive không conflict
    try:
        excel.DisplayAlerts = False
        wb.Save()
        excel.DisplayAlerts = True
        print('[v] Đã lưu thành công qua Excel AutoSave.')
    except Exception as e:
        print(f'[!] Lỗi khi lưu: {e}')

    # In kết quả hiện tại
    print()
    print('=== KẾT QUẢ HIỆN TẠI ===')
    for sheet_name in SHEETS_TO_UPDATE:
        try:
            ws = wb.Sheets(sheet_name)
            passed   = ws.Cells(SUMMARY_ROW, 1).Value or 0
            failed   = ws.Cells(SUMMARY_ROW, 2).Value or 0
            untested = ws.Cells(SUMMARY_ROW, 3).Value or 0
            na       = ws.Cells(SUMMARY_ROW, 4).Value or 0
            total    = ws.Cells(SUMMARY_ROW, 5).Value or 0
            print(f'  [{sheet_name}]')
            print(f'    Pass={int(passed)}  Fail={int(failed)}  Untested={int(untested)}  N/A={int(na)}  Total={int(total)}')
        except:
            pass

if __name__ == '__main__':
    run()
