import 'package:equatable/equatable.dart';

class User extends Equatable {
  const User({
    required this.id,
    required this.email,
    required this.displayName,
    required this.roleId,
    required this.roleName,
  });

  final String id;
  final String email;
  final String displayName;
  final String roleId;
  final String roleName;

  @override
  List<Object?> get props => [id, email, displayName];

  User copyWith({String? email, String? displayName, String? roleId, String? roleName}) => User(
    id: id,
    email: email ?? this.email,
    displayName: displayName ?? this.displayName,
    roleId: roleId ?? this.roleId,
    roleName: roleName ?? this.roleName,
  );
}
