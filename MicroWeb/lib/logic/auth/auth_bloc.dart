import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/data/services/local_storage.dart';
import '../../data/models/user.dart';
import '../../data/repositories/auth_repository.dart';

part 'auth_event.dart';
part 'auth_state.dart';

class AuthBloc extends Bloc<AuthEvent, AuthState> {
  AuthBloc(this._repo) : super(const AuthState.unknown()) {
    on<AppStarted>(_onAppStarted);
    on<SignInRequested>(_onSignInRequested);
    on<SignUpRequested>(_onSignUpRequested);
    on<SignOutRequested>(_onSignOutRequested);
  }

  final AuthRepository _repo;

  Future<void> _onAppStarted(AppStarted event, Emitter<AuthState> emit) async {
    final user = await _repo.currentUser();
    if (user == null) {
      emit(const AuthState.unauthenticated());
    } else {
      emit(AuthState.authenticated(user));
    }
  }

  Future<void> _onSignInRequested(SignInRequested event, Emitter<AuthState> emit) async {
    emit(state.copyWith(loading: true));
    try {
      final user = await _repo.signIn(email: event.email, password: event.password);
      emit(AuthState.authenticated(user));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      emit(const AuthState.unauthenticated());
    }
  }

  Future<void> _onSignUpRequested(SignUpRequested event, Emitter<AuthState> emit) async {
    emit(state.copyWith(loading: true));
    try {
      final succeeded = await _repo.signUp(
        email: event.email,
        password: event.password,
        displayName: event.displayName,
      );
      // On successful registration we keep the user unauthenticated and provide a flag to navigate.
      emit(state.copyWith(status: AuthStatus.unauthenticated, loading: false, error: null, registered: true));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      emit(const AuthState.unauthenticated());
    }
  }

  Future<void> _onSignOutRequested(SignOutRequested event, Emitter<AuthState> emit) async {
    await _repo.signOut();
    emit(const AuthState.unauthenticated());
  }
}
