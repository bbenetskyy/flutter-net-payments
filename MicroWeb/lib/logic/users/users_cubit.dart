import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/responses/role_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/repositories/users_repository.dart';

class UsersState extends Equatable {
  const UsersState({
    this.loading = false,
    this.users = const <UserResponse>[],
    this.error,
    this.roles = const <RoleResponse>[],
  });

  final bool loading;
  final List<UserResponse> users;
  final List<RoleResponse> roles;
  final String? error;

  UsersState copyWith({
    bool? loading,
    List<UserResponse>? users,
    List<RoleResponse>? roles,
    String? error, // pass null explicitly to clear
  }) {
    return UsersState(
      loading: loading ?? this.loading,
      users: users ?? this.users,
      error: error,
      roles: roles ?? this.roles,
    );
  }

  @override
  List<Object?> get props => [loading, users, error];
}
