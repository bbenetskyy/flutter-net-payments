// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'login_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

LoginDto _$LoginDtoFromJson(Map<String, dynamic> json) =>
    LoginDto(email: json['email'] as String?, password: json['password'] as String?);

Map<String, dynamic> _$LoginDtoToJson(LoginDto instance) {
  final val = <String, dynamic>{};
  void writeNotNull(String key, dynamic value) {
    if (value != null) {
      val[key] = value;
    }
  }

  writeNotNull('email', instance.email);
  writeNotNull('password', instance.password);
  return val;
}
