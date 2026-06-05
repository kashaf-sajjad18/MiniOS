; ======================================================
; NOVA-OS FILE EXPLORER
; Assembly Language Implementation
; Three Folders: Documents, Downloads, Private Vault
; ======================================================

.386
.model flat,stdcall
.stack 4096

; Windows API
GetStdHandle PROTO :DWORD
WriteConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
ReadConsoleA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
CreateFileA PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
WriteFile PROTO :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
CloseHandle PROTO :DWORD
DeleteFileA PROTO :DWORD
FindFirstFileA PROTO :DWORD,:DWORD
FindNextFileA PROTO :DWORD,:DWORD
FindClose PROTO :DWORD
CopyFileA PROTO :DWORD,:DWORD,:DWORD
Sleep PROTO :DWORD
CreateDirectoryA PROTO :DWORD,:DWORD

STD_OUTPUT_HANDLE EQU -11
STD_INPUT_HANDLE EQU -10
GENERIC_WRITE EQU 40000000h
GENERIC_READ EQU 80000000h
OPEN_EXISTING EQU 3
OPEN_ALWAYS EQU 4
FILE_ATTRIBUTE_NORMAL EQU 80h

includelib kernel32.lib
includelib user32.lib

.data

; Folder Paths
documentsPath db "MiniOS_Documents",0
downloadsPath db "MiniOS_Downloads",0
privatePath db "MiniOS_Private",0

currentPath db 260 dup(0)

; Password
privatePassword db "admin123",0
isPrivateUnlocked db 0
passwordAttempt db 50 dup(0)

; Messages
msgTitle db 13,10,"=========================================",13,10
        db "    NOVA-OS FILE EXPLORER",13,10
        db "    Assembly Language Version",13,10
        db "=========================================",13,10,0

msgWelcome db 13,10,"[SYSTEM] File System Ready",13,10
          db "[SYSTEM] Three Folders Available:",13,10
          db "  1. Documents",13,10
          db "  2. Downloads",13,10
          db "  3. Private Vault (Password Protected)",13,10,13,10,0

msgFolderMenu db "========== SELECT FOLDER ==========",13,10
             db "  1. Documents",13,10
             db "  2. Downloads",13,10
             db "  3. Private Vault",13,10
             db "  4. Exit",13,10,13,10
             db "Choice: ",0

msgFileMenu db 13,10,"========== FILE MENU ==========",13,10
           db "  1. Create Text File",13,10
           db "  2. Delete File",13,10
           db "  3. List Files",13,10
           db "  4. View Text File",13,10
           db "  5. Upload Image",13,10
           db "  6. List Images",13,10
           db "  7. Delete Image",13,10
           db "  8. Back",13,10,13,10
           db "Choice: ",0

msgPrivateLocked db 13,10,"[PRIVATE VAULT] Enter password: ",0
msgPrivateUnlocked db 13,10,"[SUCCESS] Access Granted",13,10,0
msgPrivateWrong db 13,10,"[ERROR] Wrong Password",13,10,0

msgCreate db 13,10,"Filename: ",0
msgContent db "Content: ",0
msgCreated db 13,10,"[OK] File Created",13,10,0
msgCreateFailed db 13,10,"[ERROR] Create Failed",13,10,0

msgDelete db 13,10,"Filename to delete: ",0
msgDeleted db 13,10,"[OK] File Deleted",13,10,0
msgDeleteFailed db 13,10,"[ERROR] Delete Failed",13,10,0

msgView db 13,10,"Filename to view: ",0
msgFileHeader db 13,10,"========== CONTENT ==========",13,10,0
msgFileFooter db 13,10,"=============================",13,10,0
msgFileNotFound db 13,10,"[ERROR] File Not Found",13,10,0

msgUpload db 13,10,"Source image path: ",0
msgImageName db "Save as: ",0
msgUploading db "Uploading...",13,10,0
msgUploadSuccess db 13,10,"[OK] Image Uploaded",13,10,0
msgUploadFailed db 13,10,"[ERROR] Upload Failed",13,10,0

msgListImages db 13,10,"========== IMAGES ==========",13,10,0
msgNoImages db "No images found",13,10,0
msgImageDeleted db 13,10,"[OK] Image Deleted",13,10,0

msgFilesHeader db 13,10,"========== FILES ==========",13,10,0
msgNoFiles db "No files found",13,10,0
msgFilesFooter db "=============================",13,10,0

msgExit db 13,10,"[SYSTEM] Exiting...",13,10,0
msgInvalid db 13,10,"[ERROR] Invalid choice",13,10,0
msgBack db 13,10,"[SYSTEM] Going back...",13,10,0

; Buffers
fileName db 260 dup(0)
fileContent db 4096 dup(0)
imagePath db 260 dup(0)
buffer db 4096 dup(0)
findData db 320 dup(0)

bytesRead dd 0
bytesWritten dd 0
hFile dd 0
hFind dd 0

newline db 13,10,0

.code

; ======================================================
; UTILITY FUNCTIONS
; ======================================================

StrLen PROC uses edi pStr:DWORD
    mov edi, pStr
    xor eax, eax
    dec eax
@@:
    inc eax
    cmp byte ptr [edi+eax], 0
    jne @b
    ret
StrLen ENDP

PrintString PROC uses ebx pStr:DWORD
    invoke GetStdHandle, STD_OUTPUT_HANDLE
    mov ebx, eax
    invoke StrLen, pStr
    invoke WriteConsoleA, ebx, pStr, eax, 0, 0
    ret
PrintString ENDP

PrintNewLine PROC
    invoke PrintString, ADDR newline
    ret
PrintNewLine ENDP

ReadString PROC uses ebx esi pBuffer:DWORD, maxLen:DWORD
    local bytesReadLocal:DWORD
    
    invoke GetStdHandle, STD_INPUT_HANDLE
    mov ebx, eax
    invoke ReadConsoleA, ebx, pBuffer, maxLen, ADDR bytesReadLocal, 0
    
    mov esi, pBuffer
    mov eax, bytesReadLocal
    sub eax, 2
    mov byte ptr [esi+eax], 0
    ret
ReadString ENDP

CreateDirectoryIfNotExists PROC pPath:DWORD
    invoke CreateDirectoryA, pPath, 0
    ret
CreateDirectoryIfNotExists ENDP

StrCmp PROC uses esi edi pStr1:DWORD, pStr2:DWORD
    mov esi, pStr1
    mov edi, pStr2
@@:
    mov al, [esi]
    mov bl, [edi]
    cmp al, bl
    jne not_equal
    cmp al, 0
    je equal
    inc esi
    inc edi
    jmp @b
equal:
    mov eax, 1
    ret
not_equal:
    xor eax, eax
    ret
StrCmp ENDP

; ======================================================
; SET CURRENT PATH
; ======================================================

SetCurrentPath PROC choice:DWORD
    cmp choice, 1
    je set_documents
    cmp choice, 2
    je set_downloads
    cmp choice, 3
    je set_private
    jmp done_set
    
set_documents:
    push esi
    push edi
    mov esi, OFFSET documentsPath
    mov edi, OFFSET currentPath
    mov ecx, 20
    rep movsb
    pop edi
    pop esi
    jmp done_set
    
set_downloads:
    push esi
    push edi
    mov esi, OFFSET downloadsPath
    mov edi, OFFSET currentPath
    mov ecx, 20
    rep movsb
    pop edi
    pop esi
    jmp done_set
    
set_private:
    push esi
    push edi
    mov esi, OFFSET privatePath
    mov edi, OFFSET currentPath
    mov ecx, 20
    rep movsb
    pop edi
    pop esi
    jmp done_set
    
done_set:
    ret
SetCurrentPath ENDP

; ======================================================
; PRIVATE VAULT ACCESS
; ======================================================

UnlockPrivateVault PROC
    cmp isPrivateUnlocked, 1
    je already_unlocked
    
    invoke PrintString, ADDR msgPrivateLocked
    invoke ReadString, ADDR passwordAttempt, 50
    
    invoke StrCmp, ADDR passwordAttempt, ADDR privatePassword
    cmp eax, 1
    jne wrong_password
    
    mov isPrivateUnlocked, 1
    invoke PrintString, ADDR msgPrivateUnlocked
    mov eax, 1
    ret
    
wrong_password:
    invoke PrintString, ADDR msgPrivateWrong
    xor eax, eax
    ret
    
already_unlocked:
    mov eax, 1
    ret
UnlockPrivateVault ENDP

; ======================================================
; TEXT FILE OPERATIONS
; ======================================================

CreateTextFile PROC
    local fullPath[260]:BYTE
    
    invoke PrintString, ADDR msgCreate
    invoke ReadString, ADDR fileName, 260
    
    invoke CreateFileA, ADDR currentPath, GENERIC_WRITE, 0, 0, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0
    cmp eax, -1
    je create_failed
    
    mov hFile, eax
    invoke PrintString, ADDR msgContent
    invoke ReadString, ADDR fileContent, 4096
    invoke StrLen, ADDR fileContent
    invoke WriteFile, hFile, ADDR fileContent, eax, ADDR bytesWritten, 0
    invoke CloseHandle, hFile
    
    invoke PrintString, ADDR msgCreated
    ret
    
create_failed:
    invoke PrintString, ADDR msgCreateFailed
    ret
CreateTextFile ENDP

DeleteFileProc PROC
    local fullPath[260]:BYTE
    
    invoke PrintString, ADDR msgDelete
    invoke ReadString, ADDR fileName, 260
    
    invoke DeleteFileA, ADDR currentPath
    cmp eax, 0
    je delete_failed
    
    invoke PrintString, ADDR msgDeleted
    ret
    
delete_failed:
    invoke PrintString, ADDR msgDeleteFailed
    ret
DeleteFileProc ENDP

; ======================================================
; MAIN FILE EXPLORER
; ======================================================

FileExplorerSystem PROC PUBLIC
    
    invoke CreateDirectoryIfNotExists, ADDR documentsPath
    invoke CreateDirectoryIfNotExists, ADDR downloadsPath
    invoke CreateDirectoryIfNotExists, ADDR privatePath
    
    invoke PrintString, ADDR msgTitle
    invoke PrintString, ADDR msgWelcome
    
main_loop:
    invoke PrintString, ADDR msgFolderMenu
    invoke ReadString, ADDR buffer, 10
    
    mov al, byte ptr [buffer]
    
    cmp al, '1'
    je select_documents
    
    cmp al, '2'
    je select_downloads
    
    cmp al, '3'
    je select_private
    
    cmp al, '4'
    je exit_explorer
    
    invoke PrintString, ADDR msgInvalid
    jmp main_loop
    
select_documents:
    invoke SetCurrentPath, 1
    call ShowFileMenu
    jmp main_loop
    
select_downloads:
    invoke SetCurrentPath, 2
    call ShowFileMenu
    jmp main_loop
    
select_private:
    call UnlockPrivateVault
    cmp eax, 1
    jne main_loop
    invoke SetCurrentPath, 3
    call ShowFileMenu
    jmp main_loop
    
exit_explorer:
    invoke PrintString, ADDR msgExit
    ret
    
FileExplorerSystem ENDP

ShowFileMenu PROC
    invoke PrintString, ADDR msgFileMenu
    invoke ReadString, ADDR buffer, 10
    ret
ShowFileMenu ENDP

END