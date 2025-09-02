// Data transfer object for login
// Generated helper code will be in login_dto.g.dart via json_serializable

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'login_dto.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class LoginDto {
  /// Returns a new [LoginDto] instance.
  LoginDto({this.email, this.password});

  @JsonKey(name: r'email', required: false, includeIfNull: false)
  final String? email;

  @JsonKey(name: r'password', required: false, includeIfNull: false)
  final String? password;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is LoginDto && other.email == email && other.password == password;

  @override
  int get hashCode => (email == null ? 0 : email.hashCode) + (password == null ? 0 : password.hashCode);

  factory LoginDto.fromJson(Map<String, dynamic> json) => _$LoginDtoFromJson(json);

  Map<String, dynamic> toJson() => _$LoginDtoToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
