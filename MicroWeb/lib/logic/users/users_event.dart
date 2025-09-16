import 'dart:async';
import 'package:equatable/equatable.dart';
import 'package:micro_web/data/models/responses/user_response.dart';

abstract class UsersEvent extends Equatable {
  const UsersEvent();
  @override
  List<Object?> get props => [];
}

class UsersRequested extends UsersEvent {
  const UsersRequested();
}

class UsersUpdateRequested extends UsersEvent {
  const UsersUpdateRequested({required this.userId, this.displayName, this.roleId, this.dateOfBirth, this.completer});

  final String userId;
  final String? displayName;
  final String? roleId;
  final DateTime? dateOfBirth;
  final Completer<UserResponse?>? completer;

  @override
  List<Object?> get props => [userId, displayName, roleId, dateOfBirth];
}
