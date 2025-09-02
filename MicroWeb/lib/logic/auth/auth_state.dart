part of 'auth_bloc.dart';

enum AuthStatus { unknown, authenticated, unauthenticated }

class AuthState extends Equatable {
  const AuthState._({required this.status, this.user, this.loading = false, this.error});

  const AuthState.unknown() : this._(status: AuthStatus.unknown);
  const AuthState.unauthenticated() : this._(status: AuthStatus.unauthenticated);
  const AuthState.authenticated(User user) : this._(status: AuthStatus.authenticated, user: user);

  final AuthStatus status;
  final User? user;
  final bool loading;
  final String? error;

  AuthState copyWith({AuthStatus? status, User? user, bool? loading, String? error}) => AuthState._(
        status: status ?? this.status,
        user: user ?? this.user,
        loading: loading ?? this.loading,
        error: error,
      );

  @override
  List<Object?> get props => [status, user, loading, error];
}
