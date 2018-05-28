github pages + gitment实现页面评论
======

一、 说在前面的话 
----------------
  + gitment 使用github的oauth app协助认证授权，只允许github登录用户评论 ,评论支持github flavored markdown；
  
  + gitment 采用github issues作为评论，这个有些人认可有些人不认可觉得滥用了，见仁见智吧 -.-
  
  + 本文旨在记录踩坑史...为像我这样github新手提供点帮助，老手请自觉忽略本文 -.- 
   
二、 实现过程
---------------
#### 准备工作 
 + 新建自己的仓库，设置github pages: 
 
   1. 新建repository的时候注意，命名尽量和自己的github用户名一致,采用 \[username.github.io\]，比如
      > ![img](https://jqhgit.github.io/res/zzz/oth/name.png)
    
   2. 创建github pages,点开刚刚新建的工程的 \[Settings\]：
    
      > ![settings](https://jqhgit.github.io/res/zzz/oth/reposetting.png)
      
   3. 然后选择点击 \[Select theme\] 选择一个你喜欢的风格
    
      > ![choosetheme](https://jqhgit.github.io/res/zzz/oth/choosetheme.png)
      
      然后继续点击 \[Select theme\],然后你的主页就创建好了
      
      > ![selecttheme](https://jqhgit.github.io/res/zzz/oth/reposelecttheme.png)
      
 + 注册oauth app： 
 
   1. 点自己头像然后选择 \[Settings\]:
    
      > ![settings2](https://jqhgit.github.io/res/zzz/oth/gitsettings.png)
      
   2. 选择页面左边靠下的 \[Developer settings\]: 
    
      > ![developer settings](https://jqhgit.github.io/res/zzz/oth/developersettings.png)
      
   3. 选择如图的 \[Register a new application\]:
    
      > ![Register new app](https://jqhgit.github.io/res/zzz/oth/registeroauth.png) 
      
 + 然后填写注册的内容:
 
    注册时需要输入下图所示的参数：
    
      > ![Register](https://jqhgit.github.io/res/zzz/oth/registeroauth2.png)
      
   1. **Application name**：随意发挥；
   
   2. **Homepage URL**：
      填写你当前的repository的路径，注意仓库名和用户名不同的时候可能路径长一些； -.-
      
   3. **Authorization callback URL**：
      这个很关键，在你自己的页面请求授权的时候，oauth app为了安全性会固定回调指定这个页面，你可以填写**仓库根路径**，配合gitment,在你多个页面都需要评论时，授权完成能够正确跳转。
      
   4. 另外这些填错了后面也可以改，很方便，不用太担心（-.-），成功后会有如图的结果，重要的信息就是Client ID和Client Secret，这是你在gitment请求授权的时候需要配置的。
      
       > ![register finish](https://jqhgit.github.io/res/zzz/oth/oauthapp.png)
      
   ok！后面可以开始在实际网页中借助gitment配置你需要支持评论的页面了。
      
 + 建一个专门放issues的仓库 \[可选\] 
 
   因为gitment会采用github issues来作为评论，所以需要一个存放issues的repo，参照第一步，建一个空的仓库就行，命名可以随意，比如issues :joy:,这个是用来存放评论issues的。
      
### 配置gitment 

 + 添加gitment代码段 
 
   1.gitment实现
   
   然后在你需要做评论页面的最下端部分添加如下的代码:
    
    ```
    <div id="gitmentContainer"></div>
    <link rel="stylesheet" href="https://imsun.github.io/gitment/style/default.css">
    <script src="https://imsun.github.io/gitment/dist/gitment.browser.js"></script>
    var gitment = new Gitment(
    { 
      id: 'page name or date'
      owner: 'jqhgit',
      repo: 'issues',
      oauth: 
      {
        client_id: '7bxxx84a',
        client_secret: 'b5xxxxxxx5c4',
      },
    });
    gitment.render('gitmentContainer');
    ```
    
   2. 注意
   
    id: 是当前页面对应生成issues的名称，缺省状态下是使用当前页面的\[title\]名也就是page.title
    
    owner: 填写你的user name就行
    
    repo: 是存放评论issues的仓库，只需要写**仓库名**,不需要前面的引导串，这个也比较重要
    
    client_id client_secret: 用你在第2步最后生成的oauth的id和secret就行，不熟悉html，不知道这个地方怎么避免id secret泄漏-.-。
        
   3. 汉化
   
    上面这个是英文的评论，如果需要中文的话，可以把`<link> <script>`标签替换为:
    
    ```
    <link rel="stylesheet" href="https://billts.site/extra_css/gitment.css">
    <script src="https://billts.site/js/gitment.js"></script>     
    ```
   到这一步完成，你可以试验性访问你自己的页面了,进行测试了，你需要先点击右侧登录，登录你的github账号，然后初始化这篇文章的评论（实际就是在你的issues指定仓库创建一个issue用于提交评论）。
  
 三、 评论踩坑 
 ------------------
 + 打开你的页面能看到如下图所示(这个是中文的，英文的是Comments Not Initialized)  
    
      > ![notinit](https://jqhgit.github.io/res/zzz/oth/notinit.png)
      
   点击登录，填入你的github，如果顺利出现下图，表示你很幸运，基本没出问题-.-
      
      > ![prepare](https://jqhgit.github.io/res/zzz/oth/prepare.png)
      
   1. 如果登录过程中出现未找到(Error:Not Found)
        那可能是你的owner或者repo配置错了，请确保你的repo存在且填写的是正确的仓库名（只需要填写仓库名）
        
   2. 登录过程结束后跳到别的页面了,或者仍提示未初始化(Error:Comments Not Initialized)
        请检查oauth app的 Authorization callback URL设置的是否正确
        
 + 然后你可以开始点击初始化文章评论按钮（请确保登入的是当前配置的oauth app的github user）
 
   1. 如果验证失败(Error:validation failed)
      缺省id的情会使用当前页面的title作为issue名，issue名超过50字符限制会返回这个错误，可以用页面的时间作为id，`id: '<%= page.date %>'` 或者改用短一点的title，`title: short title`，这个还是比较简单实现的-.-
  
 + 如果都通过了，你应该可以写入一条评论试试了-.- 评论页支持markdown语法。
  
四、 传送门
---------------
+ [github flavored markdown 语法传送门](https://guides.github.com/features/mastering-markdown?_blank)
+ [其它人踩坑史](https://www.jianshu.com/p/57afa4844aaa)
   
      
欢迎评论 -.- 
   <div id="gitmentContainer"></div>
   <link rel="stylesheet" href="https://billts.site/extra_css/gitment.css">
   <script src="https://billts.site/js/gitment.js"></script>
   <script src="../../gitment.js"></script>


