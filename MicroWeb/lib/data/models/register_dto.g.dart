// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'register_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RegisterDto _$RegisterDtoFromJson(Map<String, dynamic> json) =>
    $checkedCreate('RegisterDto', json, ($checkedConvert) {
      final val = RegisterDto(
        email: $checkedConvert('email', (v) => v as String?),
        password: $checkedConvert('password', (v) => v as String?),
        displayName: $checkedConvert('displayName', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$RegisterDtoToJson(RegisterDto instance) =>
    <String, dynamic>{
      'email': ?instance.email,
      'password': ?instance.password,
      'displayName': ?instance.displayName,
    };
