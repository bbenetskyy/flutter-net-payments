import 'package:equatable/equatable.dart';

import '../../data/models/responses/user_response.dart';

class UsersState extends Equatable {
  const UsersState({this.loading = false, this.items = const <UserResponse>[], this.error});

  final bool loading;
  final List<UserResponse> items;
  final String? error;

  UsersState copyWith({bool? loading, List<UserResponse>? items, String? error}) {
    return UsersState(loading: loading ?? this.loading, items: items ?? this.items, error: error);
  }

  @override
  List<Object?> get props => [loading, items, error];
}
