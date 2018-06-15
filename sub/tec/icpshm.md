ICP - Share Memory
========
进程通信（IPC,Inter-Porcess Communcation）的方式比较多，目前比较常用的有管道（Pipe）、消息队列（Message Queue）、共享内存（Share Memory）、套接字（Socket）、信号量（Semaphore）、信号（Signal）等，本篇主要关于共享内存进行了一些简要的记录。

## 一、 windows 进程程通信共享内存

### 1.同一个可执行文件或DLL使用进程共享“数据段”进行数据共享

#### 描述
  - 限制：
    需要同一个执行文件或DLL的多个实例间才会共享。

  - 优势：
    使用简单，只需要简单的定义就能够完成数据的共享。

#### 方法：
    创建自定义的共享“数据段”，如用一下的方式先定义自定义段的数据，并将自定义段属性设置为共享：

```c++
#pragma data_seg("MySectionName")
int g_instance_count = 0;
#pragma data_seg()
#pragma comment(linker, "/SECTION:MySectionName,RWS")
``` 

  注意“数据段”的属性有以下几种，类似Linux下文件的属性

  属性 | 意义
  ---- | ----
  READ | 可以从该段读取数据
  WRITE | 可以向该段写入数据
  EXECUTE | 可以执行该段的内容
  SHARED | 该段的内容为多个实例所共享（本质上是关闭了写时复制机制）

  在进行“数据段”属性设置时`#pragma comment(linker, "/SECTION:MySectionName,RWS")`只需要标明大写首字母即可，如`RWS`表示`READ|WRITE|SHARED`。如上面共享数据段内的属性`g_instance_count`就会在定义了这一数据段的同一个执行文件或DLL多份实例间共享数据。虽然可以很容易的进行简单的数据共享，但是限制较多，可能某些情况下能用的着吧？ :joy: 

  然后你就可以用这个属性干点事情比如，在每个进程开启的时候递增1，在进程退出的时候减少1，用于统计当前开启了多少个同类进程 -.-，这只是一个简单实例，demo就在下面。

* 这个东西在linux下试验了并没有生效-.- 所以列在了windows段下。

+ **demo**

    [[**demo源码 点击直达**]](https://github.com/jqhgit/jqhgit.github.io/tree/master/demo/tec/icpshm)
    
    源码见：share_data.cpp，，可以添加到控制台空工程编译即可-.-。
    或者：直接使用data_seg_share.exe运行查看效果。

+ **相关资料**

    [[**内存映射文件**]](https://www.cnblogs.com/5iedu/p/4926309.html)


### 2.内存映射(Memory Mapping)

#### **描述**

  - **限制**：
    1.Linux无法使用，仅限于Windows系统；
    2.操作较为复杂，需编编码控制较多的状态。 

  - **优势**：
    1.可以在同机器甚至是局域网及其任意进程间实现数据共享；

 
#### **采用windows sdk api ：CreateFileMapping来进行文件/内存映射实现数据共享**

   CreateFileMapping原型如下:

    ```C++
    HANDLE CreateFileMapping(
    HANDLE hFile,                       //物理文件句柄
    LPSECURITY_ATTRIBUTES lpAttributes, //安全设置
    DWORD flProtect,                    //保护设置
    DWORD dwMaximumSizeHigh,            //高位文件大小
    DWORD dwMaximumSizeLow,             //低位文件大小
    LPCTSTR lpName                      //共享内存名称
    );
    ```

    **hFile**

    指定的需要被映射到内存的物理文件句柄，如果指定`INVALID_HANDLE_VALUE`则会**页面文件**上建立一个文件无关的映射。本demo内部就是采用`INVALID_HANDLE_VALUE`来创建一块文件无关的内存映射进行数据共享。

    **lpAttributes**

    安全设置，一般设置为NULL就行。

    **flProtect**

    对共享文件的保护设置，包括但不限于以下：

    属性    |     意义
    --------------|-------------------------
    PAGE_READONLY | 以只读方式打开映射
    PAGE_READWRITE | 以可读、可写方式打开映射
    PAGE_WRITECOPY | 为写操作留下备份

    **dwMaximumSizeHigh**

    高位文件大小，指定文件映射长度的高32位，32位进程一般用不到，可以设置0。
 
    **dwMaximumSizeLow**

    低位文件大小，指定文件映射长度的低32位。也就是待映射文件的大小，在不指定有效物理文件句柄的情况下，需要指定待大小。在指定了有效物理文件句柄而设置为0，则会使用物理文件实际长度。

    **lpName**

    内存/文件映射的名称也是id，如果已经有一个同名的文件映射函数将会打开它，而不是新建一个文件/内存映射。

    **return**

    函数将返回创建的文件映射对象句柄，如果失败返回`INVALID_HANDLE_VALUE`。



#### **使用`MapViewOfFile`函数将文件映射对象映射到当前应用程序的地址空间**

通俗的将就是在你的程序里拿到文件映射段的地址,函数原型如下：
  
  ```C++
  LPVOID WINAPI MapViewOfFile(
　　__in HANDLE hFileMappingObject,     //文件映射对象句柄
　　__in DWORD dwDesiredAccess,         //文件映射对象的访问方式
　　__in DWORD dwFileOffsetHigh,        //文件映射相相对起始地址的高32位地址偏移量
　　__in DWORD dwFileOffsetLow,         //文件映射相相对起始地址的低32位地址偏移量
　　__in SIZE_T dwNumberOfBytesToMap    //文件映射的字节数
　　);
  ```

  **hFileMappingObject**

  文件映射对象句柄,一般情况下传入`CreateFileMapping`返回的句柄即可。

  **dwDesiredAccess**

  文件映射对象的访问方式,包括但不限于一下
          
    属性                |     意义
    --------------------|-------------------------
    FILE_MAP_READ       |  可以读取文件.在调用CreateFileMapping时可以传入PAGE_READONLY或PAGE_READWRITE保护属性
    FILE_MAP_WRITE      |  可以读取文件.在调用CreateFileMapping时可以传入PAGE_READONLY或PAGE_READWRITE保护属性PAGE_READWRITE保护属性
    FILE_MAP_ALL_ACCESS |   `FILE_MAP_WRITE | FILE_MAP_READ`

  **dwFileOffsetHigh**

  文件映射相相对起始地址的高32位地址偏移量，一般设置为0。

  **dwFileOffsetLow**

  文件映射相相对起始地址的低32位地址偏移量，根据实际分段偏移量设置。

  **dwNumberOfBytesToMap**

  文件映射的字节数，函数将会按照设定的地址偏移处映射指定字节数的数据到地址空间。

  **return**

  返回映射文件指定偏移位置的数据地址，后面可以对数据进行操作了。

  另外在使用完内存映射文件后，需要使用UnmapViewOfFile断开文件映射对象到地址空间的映射，或在需要关闭内存映射文件时使用CloseHandle关闭指定的内存映射文件对象。


 - **demo**

    [[**demo源码 点击直达**]](https://github.com/jqhgit/jqhgit.github.io/tree/master/demo/tec/icpshm)
    
    源码见：file_mapping.cpp，可以添加到控制台空工程编译即可-.-。
    或者使用file_mapping.exe直接查看效果。

  - **另外**

    本例仅描述了CreateFileMapping在进程间共享内存的应用，次api函数还有更多更强大的用法。CreateFileMaping+MapViewOfFile常见于使用文件映射来进行大文件的操作，可以有效降低io并节省及其内存。

## 二、 Linux 进程通信共享内存

### 利用共享内存函数进行进程间数据共享

#### **描述**
  - **限制**：
    1.windows下无法使用；
    2.操作较为复杂，同样需要编码控制较多的状态。

  - **优势**：
    1.内存共享不受无情缘关系限制；
    2.Linux版本通用。

#### **方法**:

    使用shmget、shmat、shmdt、shmctl共享内存函数进行数据共享。

    至于函数原型就借用在百科上扣的图了 -.-，如下：

  > ![shmget](../../res/tec/icpshm/shmget.png)

    在需要共享数据时，可以首先使用`shmget`来创建或者查找已经存在的共享内存。其中`key`参数理解为为内核中共享内存的编号，也就是一把访问共享内存的钥匙了；`size`则是创建的共享内存的大小；`shmflg` 参数在输入`0`的时候可以用来检测共享内存是否已经创建，正常创建时可以指定IPC_CREAT（没有回自动创建）。


  > ![shmat](../../res/tec/icpshm/shmat.png)

    shmat则是获取共享内存的地址，通常返回一个`void *`的首地址指针。获取之后就能够对共享内存块进行读写操作了。


  > ![shmdt](../../res/tec/icpshm/shmdt.png)

  > ![shmctl](../../res/tec/icpshm/shmctl.png)

  具体的使用如果有兴趣可以在demo里面瞅瞅，虽然很low，但是意思还是有的... :joy: 

+ **demo**

    这个demo和共享数据段类似，就只是共享了一个int数值，用于统计开启了多少个进程，这个数值会在进程开启的时候+1，在进程退出时-1。
    
    再放一次传送门...:

    [[**demo源码 点击直达**]](https://github.com/jqhgit/jqhgit.github.io/tree/master/demo/tec/icpshm)

    你需要在linux下编译一下share_data.cpp，注意执行`dos2unix`进行格式转换，不然你会看到n多错误。然后运行多个程序就能看到效果了（CentOS/RedHat 最好是用多个终端同时运行，不然一个终端后台进程同时输出日志的话比较难受，如果是ubantu...应该没问题），或者直接使用share_memory运行。


   <div id="gitmentContainer"></div>
   <link rel="stylesheet" href="https://billts.site/extra_css/gitment.css">
   <script src="https://billts.site/js/gitment.js"></script>
   <script src="../../gitment.js"></script>
