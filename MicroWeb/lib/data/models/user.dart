import 'package:equatable/equatable.dart';

class User extends Equatable {
  const User({required this.id, required this.email, required this.displayName});

  final String id;
  final String email;
  final String displayName;

  @override
  List<Object?> get props => [id, email, displayName];

  User copyWith({String? email, String? displayName}) => User(
        id: id,
        email: email ?? this.email,
        displayName: displayName ?? this.displayName,
      );
}
