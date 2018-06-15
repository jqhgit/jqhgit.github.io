#define ACTIVE_FM

#ifdef ACTIVE_FM
#include <iostream>
using namespace std;

#if defined(_WIN32) || defined(__WIN32__) || defined(WIN32)

#include <Windows.h>

void msSleep(long n)
{
	Sleep(n);
}
void instanceSubOne();
bool Exit(DWORD ctrltype)
{
	if (ctrltype == CTRL_CLOSE_EVENT)
	{
		instanceSubOne();
	}
	return true;
}
void listenCloseEvent()
{
	if (!SetConsoleCtrlHandler((PHANDLER_ROUTINE)Exit, true))
	{
		exit(0);
	}
}
const WCHAR* mmap_name = L"MM_NAME";
HANDLE g_mmp_handle = INVALID_HANDLE_VALUE;
char* g_map_buff = NULL;
const int buf_size = 5;
void checkAndCreateMemoryMapping()
{
	if (g_map_buff != NULL || g_mmp_handle != INVALID_HANDLE_VALUE)
		return;

	g_mmp_handle = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, buf_size, mmap_name);
	if (INVALID_HANDLE_VALUE == g_mmp_handle)
	{
		cout << "CreateFileMapping failed." << endl;
		return;
	}
	g_map_buff = (char*)MapViewOfFile(g_mmp_handle, FILE_MAP_ALL_ACCESS, 0, 0, buf_size);
	if (NULL == g_map_buff)
	{
		cout << "MapViewOfFile failed." << endl;
		return;
	}
}
void unmapMeomryMapping()
{
	if (NULL != g_map_buff)
	{
		UnmapViewOfFile(g_map_buff);
		g_map_buff = NULL;
	}
}
void closeMemoryMapping()
{
	if (NULL != g_map_buff)
	{
		UnmapViewOfFile(g_map_buff);
		g_map_buff = NULL;
	}
	if (g_mmp_handle != INVALID_HANDLE_VALUE)
	{
		CloseHandle(g_mmp_handle);
		g_mmp_handle = INVALID_HANDLE_VALUE;
	}
}
int getInstanceCount()
{
	checkAndCreateMemoryMapping();
	if (NULL == g_map_buff)
		return -1;

	return atoi(g_map_buff);
}
void instanceAddOne()
{
	checkAndCreateMemoryMapping();
	if (NULL == g_map_buff)
		return;

	sprintf_s(g_map_buff, buf_size, "%d", atoi(g_map_buff) + 1);
}
void instanceSubOne()
{
	checkAndCreateMemoryMapping();
	if (NULL == g_map_buff)
		return;

	int result_count = atoi(g_map_buff) - 1;
	sprintf_s(g_map_buff, buf_size, "%d", result_count);

	if (result_count < 1)
	{
		closeMemoryMapping();
	}
	else
	{
		unmapMeomryMapping();
	}
}
#else
void listenCloseEvent() {}
void instanceAddOne() {}
int getInstanceCount() { return -1; }
#endif

int main()
{
	listenCloseEvent();
	instanceAddOne();

	int count = -1;
	while (true)
	{
		int cur = getInstanceCount();
		if (count != cur)
		{
			count = cur;
			cout << "当前开启的进程数:" << count << endl;
		}

		Sleep(100);
	}
}

#endif