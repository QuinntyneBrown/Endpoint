@echo off
cd /d %~dp0..\..
for /r %%f in (tmp*-cwd) do del "%%f"
