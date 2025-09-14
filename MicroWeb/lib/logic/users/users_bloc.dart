import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/logic/users/users_cubit.dart';
import 'package:micro_web/logic/users/users_event.dart';

import '../../data/models/responses/user_response.dart';
import '../../data/repositories/users_repository.dart';

class UsersBloc extends Bloc<UsersEvent, UsersState> {
  UsersBloc(this._repo) : super(const UsersState()) {
    on<UsersRequested>(_onRequested);
  }

  final UsersRepository _repo;

  Future<void> load() async => add(const UsersRequested());

  Future<void> _onRequested(UsersRequested event, Emitter<UsersState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final me = await _repo.getMe();
      final list = await _repo.listUsers();
      final roles = await _repo.listRoles();
      list.removeWhere((u) => (u.id != null && u.id == me.id));
      emit(state.copyWith(loading: false, users: list, error: null, roles: roles));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }
}
