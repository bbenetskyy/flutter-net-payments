part of 'accounts_bloc.dart';

abstract class AccountsEvent extends Equatable {
  const AccountsEvent();
  @override
  List<Object?> get props => [];
}

class AccountsRequested extends AccountsEvent {
  const AccountsRequested();
}

class AccountCreateRequested extends AccountsEvent {
  const AccountCreateRequested(this.request, this.completer);
  final CreateAccountRequest request;
  final Completer<AccountResponse?> completer;
}

class AccountDeleteRequested extends AccountsEvent {
  const AccountDeleteRequested(this.id, this.completer);
  final String id;
  final Completer<void> completer;
}
