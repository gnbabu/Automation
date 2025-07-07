import {
  ClassInfo,
  IAutomationDataSection,
  IAutomationFlow,
  IQueueInfo,
  IUser,
  IUserRole,
  LibraryInfo,
  LibraryMethodInfo,
  PagedResult,
} from '@interfaces';

export const UserMapper = {
  fromApi(data: any): IUser {
    return {
      userId: data.userId,
      userName: data.userName,
      password: data.password,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      photo: data.photo,
      active: data.active,
      roleId: data.roleId,
      roleName: data.roleName,
    };
  },

  toApi(user: IUser): any {
    return {
      userId: user.userId,
      userName: user.userName,
      password: user.password,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      photo: user.photo,
      active: user.active,
      roleId: user.roleId,
    };
  },

  empty(): IUser {
    return {
      userId: undefined,
      userName: '',
      password: '',
      firstName: '',
      lastName: '',
      email: '',
      photo: undefined,
      active: false,
      roleId: null,
      roleName: '',
    };
  },
};

export const UserRoleMapper = {
  fromApi(data: any): IUserRole {
    return {
      roleId: data.roleId,
      roleName: data.roleName,
    };
  },
};

export const LibraryMethodInfoMapper = {
  fromApi(data: any): LibraryMethodInfo {
    return {
      methodName: data.methodName,
    };
  },
};

export const ClassInfoMapper = {
  fromApi(data: any): ClassInfo {
    return {
      className: data.className,
      methods: (data.methods || []).map((m: any) =>
        LibraryMethodInfoMapper.fromApi(m)
      ),
    };
  },
};

export const LibraryInfoMapper = {
  fromApi(data: any): LibraryInfo {
    return {
      libraryName: data.libraryName,
      classes: (data.classes || []).map((c: any) => ClassInfoMapper.fromApi(c)),
    };
  },
};

export const AutomationFlowMapper = {
  fromApi(data: any): IAutomationFlow {
    return {
      flowName: data.flowName,
    };
  },
};

export const AutomationDataSectionMapper = {
  fromApi(data: any): IAutomationDataSection {
    return {
      sectionId: data.sectionId,
      sectionName: data.sectionName,
      testContent: data.testContent,
      id: data.id,
    };
  },
};

export const QueueInfoMapper = {
  fromApi(data: any): IQueueInfo {
    return {
      queueId: data.queueId,
      tagName: data.tagName,
      empId: data.empId,
      queueName: data.queueName,
      queueDescription: data.queueDescription,
      productLine: data.productLine,
      queueStatus: data.queueStatus,
      createdDate: data.createdDate ? new Date(data.createdDate) : undefined,
      id: data.id,
      libraryName: data.libraryName,
      className: data.className,
      methodName: data.methodName,
      userId: data.userId,
      userName: data.userName,
    };
  },
  toApi(queue: IQueueInfo): any {
    return {
      id: queue.id,
      tagName: queue.tagName,
      empId: queue.empId,
      queueName: queue.queueName,
      queueDescription: queue.queueDescription,
      productLine: queue.productLine,
      queueStatus: queue.queueStatus,
      libraryName: queue.libraryName,
      className: queue.className,
      methodName: queue.methodName,
      userId: queue.userId,
    };
  },
  empty(): IQueueInfo {
    return {
      id: 0,
      queueId: '',
      tagName: '',
      empId: '',
      queueName: '',
      queueDescription: '',
      productLine: '',
      queueStatus: '',
      createdDate: undefined,
      libraryName: '',
      className: '',
      methodName: '',
      userId: 0,
      userName: '',
    };
  },
  mapPagedResult(apiResult: any): PagedResult<IQueueInfo> {
    return {
      data: Array.isArray(apiResult.data)
        ? apiResult.data.map(QueueInfoMapper.fromApi)
        : [],
      totalCount: apiResult.totalCount ?? 0,
    };
  },
};

export const Mappers = {
  UserMapper: UserMapper,
  UserRoleMapper: UserRoleMapper,
  LibraryInfoMapper: LibraryInfoMapper,
  ClassInfoMapper: ClassInfoMapper,
  LibraryMethodInfoMapper: LibraryMethodInfoMapper,
  AutomationFlowMapper: AutomationFlowMapper,
  AutomationDataSectionMapper: AutomationDataSectionMapper,
  QueueInfoMapper: QueueInfoMapper,
};
