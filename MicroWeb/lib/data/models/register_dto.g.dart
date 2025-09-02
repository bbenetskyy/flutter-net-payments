// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'register_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RegisterDto _$RegisterDtoFromJson(Map<String, dynamic> json) => RegisterDto(
  email: json['email'] as String?,
  password: json['password'] as String?,
  displayName: json['displayName'] as String?,
);

Map<String, dynamic> _$RegisterDtoToJson(RegisterDto instance) {
  final val = <String, dynamic>{};
  void writeNotNull(String key, dynamic value) {
    if (value != null) {
      val[key] = value;
    }
  }

  writeNotNull('email', instance.email);
  writeNotNull('password', instance.password);
  writeNotNull('displayName', instance.displayName);
  return val;
}
