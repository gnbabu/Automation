import {
  ClassInfo,
  IAutomationData,
  IAutomationDataSection,
  IAutomationFlow,
  IPriorityStatus,
  ITimeZone,
  IUser,
  IUserRole,
  IUserStatus,
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
      priorityId: data.priorityId,
      priorityName: data.priorityName,
      lastLogin: data.lastLogin ? new Date(data.lastLogin) : undefined,
      timeZone: data.timeZone,
      timeZoneName: data.timeZoneName,
      status: data.status,
      statusName: data.statusName,
      phoneNumber: data.phoneNumber,
      twoFactor: data.twoFactor,
      teams: data.teams,
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
      roleName: user.roleName,
      priorityId: user.priorityId,
      priorityName: user.priorityName,
      lastLogin: user.lastLogin ? new Date(user.lastLogin) : undefined,
      timeZone: user.timeZone,
      timeZoneName: user.timeZoneName,
      status: user.status,
      statusName: user.statusName,
      phoneNumber: user.phoneNumber,
      twoFactor: user.twoFactor,
      teams: user.teams,
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
      active: true,
      roleId: null,
      roleName: '',
      priorityId: undefined,
      priorityName: '',
      lastLogin: undefined,
      timeZone: undefined,
      timeZoneName: '',
      status: undefined,
      statusName: '',
      phoneNumber: '',
      twoFactor: false,
      teams: '',
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

export const UserStatusMapper = {
  fromApi(data: any): IUserStatus {
    return {
      statusId: data.statusId,
      statusName: data.statusName,
    };
  },
};

export const TimeZoneMapper = {
  fromApi(data: any): ITimeZone {
    return {
      timeZoneId: data.timeZoneId,
      timeZoneName: data.timeZoneName,
      utcOffsetMinutes: data.utcOffsetMinutes,
      description: data.description,
    };
  },
};

export const PriorityStatusMapper = {
  fromApi(data: any): IPriorityStatus {
    return {
      priorityId: data.priorityId,
      priorityName: data.priorityName,
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
      flowName: data.flowName,
    };
  },
};

export const AutomationDataMapper = {
  fromApi(data: any): IAutomationData {
    return {
      id: data.id,
      sectionId: data.sectionId,
      sectionName: data.sectionName,
      testContent: data.testContent,
      userId: data.userId,
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
  AutomationDataMapper: AutomationDataMapper,
  UserStatusMapper: UserStatusMapper,
  TimeZoneMapper: TimeZoneMapper,
  PriorityStatusMapper: PriorityStatusMapper,
};
