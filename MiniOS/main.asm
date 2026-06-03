.386
.model flat,stdcall
.stack 4096
ExitProcess PROTO :DWORD
BootSystem PROTO


ShellSystem PROTO

includelib kernel32.lib

.code

main PROC

  call BootSystem

call ShellSystem

    invoke ExitProcess,0

main ENDP

END main