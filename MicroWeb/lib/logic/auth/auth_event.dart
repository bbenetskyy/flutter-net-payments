part of 'auth_bloc.dart';

abstract class AuthEvent extends Equatable {
  const AuthEvent();
  @override
  List<Object?> get props => [];
}

class AppStarted extends AuthEvent {}

class SignInRequested extends AuthEvent {
  const SignInRequested({required this.email, required this.password});
  final String email;
  final String password;
}

class SignUpRequested extends AuthEvent {
  const SignUpRequested({required this.email, required this.password, this.displayName});
  final String email;
  final String password;
  final String? displayName;
}

class SignOutRequested extends AuthEvent {}
