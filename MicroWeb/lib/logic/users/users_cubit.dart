import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/responses/user_response.dart';
import '../../data/repositories/users_repository.dart';

class UsersState extends Equatable {
  const UsersState({this.loading = false, this.items = const <UserResponse>[], this.error});

  final bool loading;
  final List<UserResponse> items;
  final String? error;

  UsersState copyWith({
    bool? loading,
    List<UserResponse>? items,
    String? error, // pass null explicitly to clear
  }) {
    return UsersState(loading: loading ?? this.loading, items: items ?? this.items, error: error);
  }

  @override
  List<Object?> get props => [loading, items, error];
}

class UsersCubit extends Cubit<UsersState> {
  UsersCubit(this._repo) : super(const UsersState());

  final UsersRepository _repo;

  Future<void> load() async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final me = await _repo.getMe();
      final list = await _repo.listUsers();
      // Exclude current user by id
      list.removeWhere((u) => (u.id != null && u.id == me.id));
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }
}
