using StarRail_Launcher.Models;
using System.Collections.Generic;

namespace StarRail_Launcher.Service.IService
{
    public interface IUserDataService
    {
        /// <summary>
        /// 读取用户数据文件到List
        /// </summary>
        /// <returns></returns>
        List<UserListModel> ReadUserList();
    }
}
